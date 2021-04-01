using discord_rpc_tidal.Logging;
using DiscordRPC;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using SimpleTidalApi;
using SimpleTidalApi.Model;

namespace discord_rpc_tidal
{
    class DiscordRPC : IDisposable
    {
        private const string LARGEIMAGEKEY = "tidal";
        private const string LARGEIMAGETEXT = "TIDAL";
        private const string APPID = "735159392554713099";
        private const int MAXTIMEDIFFERENCE = 1000;
        private const int KEYREFRESHINTERVAL = 120 * 1000;

        private readonly TidalListener TidalListener;

        private DiscordRpcClient Discord = new DiscordRpcClient(APPID, -1, new DiscordLogger()) { SkipIdenticalPresence = false };
        private LoginKey LoginKey;
        private Timer RefreshKeyTimer;

        public DiscordRPC(TidalListener tidalListener)
        {
            TidalListener = tidalListener;
            Discord.Initialize();

            TidalListener.SongChanged += TidalListener_SongChanged;
            TidalListener.TimecodeChanged += TidalListener_TimecodeChanged;

            Discord.OnReady += Discord_OnReady;

            RefreshKeyTimer = new Timer(KEYREFRESHINTERVAL) { AutoReset = true };
            RefreshKeyTimer.Elapsed += (sender, args) => LoginKey = null;
            RefreshKeyTimer.Start();
        }

        private async void TidalListener_SongChanged(string oldSong, string newSong)
        {
            if (newSong == null) // clear RPC if no song is playing
            {
                Discord.ClearPresence();
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
                presence.Timestamps = new Timestamps(DateTime.UtcNow.AddSeconds(-TidalListener.CurrentTimecode.Value));

            Discord.SetPresence(presence);

            // query TIDAL API for the track's link
            var url = await Task.Run(async () =>
            {
                if (LoginKey == null)
                {
                    var key = TidalClient.GetAccessTokenFromTidalDesktop();
                    if (key.Item1 != null)
                    {
                        Trace.TraceInformation("Could not get the key from TIDAL for the following Reason: " + key.Item1);
                        return null;
                    }

                    if (key.Item2 == null)
                        return null;

                    LoginKey = key.Item2;
                }

                var searchResult = await TidalClient.Search(LoginKey, newSong, 1, QueryFilter.TRACK);

                if (searchResult.Item1 != null)
                {
                    Trace.TraceInformation("Search query for the track failed for the following reason: " + searchResult.Item1);
                    return null;
                }

                if (searchResult.Item2 == null || searchResult.Item2.Tracks == null || searchResult.Item2.Tracks.Count == 0)
                    return null;

                return searchResult.Item2.Tracks[0].Url;
            });

            // add a button to open the song in RPC
            var currentPresence = Discord.CurrentPresence;
            if (url != null && Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute) && TidalListener.CurrentSong == newSong && currentPresence != null)
            {
                var modifiedPresence = currentPresence.Clone();
                modifiedPresence.Buttons = new Button[]
                {
                    new Button
                    {
                        Label = "Play on TIDAL",
                        Url = url
                    }
                };

                Discord.SetPresence(modifiedPresence);
            }
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

        private void Discord_OnReady(object sender, global::DiscordRPC.Message.ReadyMessage args)
        {
            // Discord has just been started, update the RPC
            TidalListener_SongChanged(null, TidalListener.CurrentSong);
        }

        public void Dispose()
        {
            if (TidalListener != null)
            {
                TidalListener.SongChanged -= TidalListener_SongChanged;
                TidalListener.TimecodeChanged -= TidalListener_TimecodeChanged;
            }

            Discord.ClearPresence();
            Discord.Dispose();

            RefreshKeyTimer.Dispose();
        }
    }
}
