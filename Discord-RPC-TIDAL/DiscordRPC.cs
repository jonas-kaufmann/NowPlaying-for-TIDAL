using DiscordRPC;
using System;
using discord_rpc_tidal.Data;
using discord_rpc_tidal.Discord;
using discord_rpc_tidal.Utils;

namespace discord_rpc_tidal
{
    class DiscordRPC : IDisposable
    {
        private readonly TidalListener TidalListener;
        private readonly DiscordRpcClient Discord = new(AppConfig.DiscordAppId);

        public DiscordRPC(TidalListener tidalListener)
        {
            TidalListener = tidalListener;
            Discord.Initialize();

            TidalListener.SongChanged += TidalListener_SongChanged;
            TidalListener.TimecodeChanged += TidalListener_TimecodeChanged;

            Discord.OnReady += Discord_OnReady;
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
                Details = GeneralUtils.CutDownStringToByteSize(songinfo.Item1, 128),
                State = GeneralUtils.CutDownStringToByteSize(songinfo.Item2, 128),
                Assets = new Assets
                {
                    LargeImageKey = Constants.DiscordRpcDefaultLargeImageKey,
                    LargeImageText = Constants.DiscordRpcDefaultLargeImageText
                }
            };

            if (TidalListener.CurrentTimecode.HasValue)
                presence.Timestamps = new Timestamps(DateTime.UtcNow.AddSeconds(-TidalListener.CurrentTimecode.Value));

            Discord.SetPresence(presence);

            // query TIDAL API for infos about the track
            var track = await TidalApi.QueryTrack(songinfo.Item1, songinfo.Item2);

            if (track == null)
                return;

            var currentPresence = Discord.CurrentPresence;

            // add album cover if available
            if (AssetManager.Available(track.Album))
                currentPresence.Assets.LargeImageKey = track.Album.ID.ToString();
            else
                _ = AssetManager.UploadIfNotExists(track.Album);

            // add a button to open the song in RPC
            if (track.Url == null || !Uri.IsWellFormedUriString(track.Url, UriKind.RelativeOrAbsolute) ||
                TidalListener.CurrentSong != newSong || currentPresence == null) return;

            var modifiedPresence = currentPresence.Clone();
            modifiedPresence.Buttons = new[]
            {
                new Button
                {
                    Label = "Play on TIDAL",
                    Url = track.Url
                }
            };

            Discord.SetPresence(modifiedPresence);
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
                    if (comparison <= 0 && currentPresenceTime.Value.Subtract(newStartTime).TotalMilliseconds <=
                        Constants.DiscordRpcTimecodeDiff // currentPresenceTime more recent than newStartTime
                        || comparison > 0 && newStartTime.Subtract(currentPresenceTime.Value).TotalMilliseconds <=
                        Constants.DiscordRpcTimecodeDiff) // newStartTime more recent than currentPresenceTime
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
            if (TidalListener.CurrentSong != null)
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
        }
    }
}