using discord_rpc_tidal.Logging;
using DiscordRPC;
using System;

namespace discord_rpc_tidal
{
    class DiscordRPC : IDisposable
    {
        private const string LARGEIMAGEKEY = "tidal";
        private const string LARGEIMAGETEXT = "TIDAL";
        private const string APPID = "735159392554713099";
        private const int MAXTIMEDIFFERENCE = 1000;

        private readonly TidalListener TidalListener;

        private DiscordRpcClient Discord = new DiscordRpcClient(APPID, -1,new DiscordLogger())
        {
            SkipIdenticalPresence = true
        };

        public DiscordRPC(TidalListener tidalListener)
        {
            TidalListener = tidalListener;
            TidalListener.SongChanged += TidalListener_SongChanged;
            TidalListener.TimecodeChanged += TidalListener_TimecodeChanged;

            Discord.Initialize();
        }

        private void TidalListener_SongChanged(string oldSong, string newSong)
        {
            if (newSong == null) // clear RPC if no song is playing
            {
                Discord.SetPresence(null);
                return;
            }

            var songinfo = TidalListener.GetSongAndArtist();

            var presence = new RichPresence()
            {
                Details = songinfo.Item1,
                State = songinfo.Item2,
                Assets = new Assets
                {
                    LargeImageKey = LARGEIMAGEKEY,
                    LargeImageText = LARGEIMAGETEXT
                }
            };

            if (TidalListener.CurrentTimecode.HasValue)
                presence.Timestamps =  new Timestamps(DateTime.UtcNow.AddSeconds(-TidalListener.CurrentTimecode.Value));

            Discord.SetPresence(presence);
        }

        private void TidalListener_TimecodeChanged(double? oldTimecode, double? newTimeCode)
        {
            if (Discord.CurrentPresence == null || TidalListener.CurrentSong == null)
                return;

            if (!newTimeCode.HasValue)
                Discord.UpdateClearTime();
            else
            {
                // only update if the time shown is off by more than one second
                var newStartTime = DateTime.UtcNow.AddSeconds(-newTimeCode.Value);

                if (Discord.CurrentPresence.HasTimestamps() && Discord.CurrentPresence.Timestamps.Start.HasValue)
                {
                    var currentPresenceTime = Discord.CurrentPresence.Timestamps.Start;
                    var comparison = newStartTime.CompareTo(currentPresenceTime);

                    // don't update when difference between time shown and actual is within margin of error to avoid visual annoyances
                    if (comparison == 0
                        || (comparison <= 0 && currentPresenceTime.Value.Subtract(newStartTime).TotalMilliseconds <= MAXTIMEDIFFERENCE) // currentPresenceTime more recent than newStartTime
                        || (comparison >= 0 && newStartTime.Subtract(currentPresenceTime.Value).TotalMilliseconds <= MAXTIMEDIFFERENCE)) // newStartTime more recent than currentPresenceTime
                    {
                        return;
                    }
                }

                Discord.UpdateStartTime(newStartTime);
            }
        }

        public void Dispose()
        {
            Discord.SetPresence(null);
            Discord.ClearPresence();
            Discord.Dispose();
        }
    }
}
