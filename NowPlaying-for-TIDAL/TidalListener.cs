using Squalr.Engine.DataTypes;
using Squalr.Engine.Memory;
using Squalr.Engine.OS;
using Squalr.Engine.Scanning.Scanners;
using Squalr.Engine.Scanning.Scanners.Constraints;
using Squalr.Engine.Scanning.Snapshots;
using Squalr.Engine.Snapshots;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace nowplaying_for_tidal
{
    class TidalListener : IDisposable
    {
        #region Constants

        private const string Processname = "TIDAL";
        private const string Splitstring = "-";
        private const int Refreshinterval = 1000;
        private const int Refreshintervaladdress = 4000;
        private const int Timecodeupperdeviation = 2 * Refreshinterval;
        private const int Timecodelowerdeviation = 3000;
        private const int Maxtimecodefails = 8;

        #endregion


        #region Properties

        public string CurrentSong { get; private set; }

        public double? CurrentTimecode { get; private set; }

        public Process Process { get; private set; }

        #endregion


        #region Events

        public delegate void SongChangedEventHandler(string oldSong, string newSong);

        public event SongChangedEventHandler SongChanged;

        public delegate void TimecodeChangedEventHandler(double? oldTimecode, double? newTimeCode);

        public event TimecodeChangedEventHandler TimecodeChanged;

        public delegate void ProcessChangedEventHandler(Process oldProcess, Process newProcess);

        public event ProcessChangedEventHandler ProcessChanged;

        #endregion


        public TidalListener()
        {
            UpdateSongInfoTimer.Elapsed += (_, _) => UpdateSongInfo();
        }


        /// <returns>(title, artist) of the currently playing song or ("", "") if unknown</returns>
        public (string, string) GetSongAndArtist()
        {
            var cut = CurrentSong.Split(Splitstring, 2, StringSplitOptions.TrimEntries);

            return cut.Length switch
            {
                1 => (cut[0], string.Empty),
                2 => (cut[0], cut[1]),
                _ => (string.Empty, string.Empty)
            };
        }

        private void UpdateProcess()
        {
            Process?.Refresh();

            if (Process == null || Process.HasExited)
            {
                TimecodeAddress = null;
                MostRecentSong = null;

                var oldProcess = Process;
                Process = null;

                // try to find new process
                foreach (var process in Process.GetProcessesByName(Processname))
                {
                    if (!string.IsNullOrWhiteSpace(process.MainWindowTitle)) // process found
                    {
                        Process = process;
                        break;
                    }
                }

                if (oldProcess != Process)
                    ProcessChanged?.Invoke(oldProcess, Process);
            }
        }

        private string MostRecentSong;
        private ulong? TimecodeAddress;
        private int TimecodeFailCount;

        private void UpdateSongInfo()
        {
            UpdateProcess();

            // do nothing when the window title is empty
            if (Process != null && string.IsNullOrEmpty(Process.MainWindowTitle))
                return;

            Stopwatch songStartTime = new Stopwatch();
            songStartTime.Start();

            // update song
            var oldSong = CurrentSong;
            var oldMostRecentSong = MostRecentSong;
            if (Process == null || Process.MainWindowTitle.Trim()
                .Contains(Processname, StringComparison.CurrentCultureIgnoreCase)) // if no song is playing
            {
                CurrentSong = null;
            }
            else
            {
                CurrentSong = Process.MainWindowTitle;
                MostRecentSong = CurrentSong;
            }

            // update timecode
            var oldTimecode = CurrentTimecode;

            if (TimecodeAddress == null || CurrentSong == null)
                CurrentTimecode = null;
            else
            {
                var value = Reader.Default.Read<double>(TimecodeAddress.Value, out var success);
                DateTime? test = null;
                try
                {
                    test = DateTime.UtcNow.AddSeconds(value);
                }
                catch (ArgumentOutOfRangeException)
                {
                    
                }
                success = success && test.HasValue;
                CurrentTimecode = success ? value : null;

                // scrap timecode address if multiple read attempts fail
                if (success)
                    TimecodeFailCount = 0;
                else
                {
                    TimecodeFailCount++;

                    if (TimecodeFailCount >= Maxtimecodefails)
                    {
                        TimecodeAddress = null;
                    }
                }
            }

            // notify subscribers
            if (oldSong != CurrentSong)
            {
                TokenSource?.Cancel(); // cancel running task to find timecode address
                SongChanged?.Invoke(oldSong, CurrentSong);
            }
            else if (CurrentTimecode == null || oldTimecode == null ||
                     Math.Abs(CurrentTimecode.Value - oldTimecode.Value) > 0.1)
                TimecodeChanged?.Invoke(oldTimecode, CurrentTimecode);

            // update timecode address if not yet set
            if (!TimecodeAddress.HasValue && oldMostRecentSong != MostRecentSong)
            {
                UpdateTimecodeAddress(songStartTime);
            }
            else
                songStartTime.Stop();
        }

        private CancellationTokenSource TokenSource;

        private void UpdateTimecodeAddress(Stopwatch songStartTime)
        {
            TimecodeAddress = null;

            TokenSource = new CancellationTokenSource();
            Task.Run(() => FindAddress(songStartTime, TokenSource.Token), TokenSource.Token);
            Trace.TraceInformation("Started task for finding timecode address.");
        }

        private void FindAddress(Stopwatch songStartTime, CancellationToken cancellationToken)
        {
            if (Process == null)
                return;

            if (Processes.Default.OpenedProcess == null || Processes.Default.OpenedProcess.Id != Process.Id)
                Processes.Default.OpenedProcess = Process;

            var dataType = DataType.Double;
            var snapshot =
                SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode
                    .FromSettings); // Use activeOrPrefilter here when new version of squalr is released
            snapshot.ElementDataType = dataType;

            var timer = new System.Timers.Timer(Refreshintervaladdress)
            {
                AutoReset = false
            };

            // read process memory and filter out addresses that fit to the current timecode
            timer.Elapsed += async (_, _) =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var scanConstraints = new ScanConstraintCollection();
                    var upperBound = (songStartTime.ElapsedMilliseconds + Timecodeupperdeviation) / 1000d;
                    var lowerBound = (songStartTime.ElapsedMilliseconds - Timecodelowerdeviation) / 1000d;
                    scanConstraints.AddConstraint(new ScanConstraint(ScanConstraint.ConstraintType.LessThanOrEqual,
                        upperBound));
                    scanConstraints.AddConstraint(new ScanConstraint(ScanConstraint.ConstraintType.GreaterThanOrEqual,
                        lowerBound));

                    var scanTask =
                        ManualScanner.Scan(snapshot, dataType, scanConstraints, null,
                            out var scanCts); // further filter snapshot (checks current values)
                    cancellationToken.Register(scanCts.Cancel);

                    snapshot = await scanTask;
                    cancellationToken.ThrowIfCancellationRequested();

                    // timecode not found
                    if (snapshot.ElementCount == 0)
                    {
                        Trace.TraceInformation("Address of timecode could not be found.");

                        timer.Dispose();
                        songStartTime.Stop();
                        return;
                    }

                    // timecode has been found
                    if (snapshot.ElementCount <= 4)
                    {
                        TimecodeAddress = snapshot[0].BaseAddress;
                        TimecodeFailCount = 0;
                        Trace.TraceInformation("Address of timecode has been found: " + $"0x{TimecodeAddress.Value:X}");
                        return;
                    }

                    timer.Start();
                }
                catch (OperationCanceledException)
                {
                    timer.Dispose();
                    songStartTime.Stop();
                }
            };

            timer.Start();
        }

        private readonly System.Timers.Timer UpdateSongInfoTimer = new System.Timers.Timer(Refreshinterval)
        {
            AutoReset = true
        };

        public void Start()
        {
            if (!UpdateSongInfoTimer.Enabled)
                UpdateSongInfoTimer.Start();
        }

        public void Stop()
        {
            if (UpdateSongInfoTimer.Enabled)
            {
                UpdateSongInfoTimer.Stop();

                var oldSong = CurrentSong;
                CurrentSong = null;
                CurrentTimecode = null;

                if (oldSong != CurrentSong)
                    SongChanged?.Invoke(oldSong, CurrentSong);
            }
        }

        public void Dispose()
        {
            Stop();
            UpdateSongInfoTimer.Dispose();
            Process?.Dispose();
        }
    }
}