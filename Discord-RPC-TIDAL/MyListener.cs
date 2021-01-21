using DiscordRPC;
using System;
using System.Timers;

namespace discord_rpc_tidal
{
    public class MyListener : IDisposable
    {
        private const int REFRESHTIME = 1000;
        private const string LARGEIMAGEKEY = "tidal";
        private const string LARGEIMAGETEXT = "TIDAL";
        private const string APPID = "735159392554713099";

        private readonly Timer Timer = new Timer(REFRESHTIME)
        {
            AutoReset = true
        };
        private DiscordRpcClient Discord = new DiscordRpcClient(APPID)
        {
            SkipIdenticalPresence = true
        };

        public MyListener()
        {
            Timer.Elapsed += Timer_Elapsed;
            Discord.Initialize();
        }


        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string songInfo = Tidal.GetCurrentlyPlaying();

            // disconnect from discord if nothing is playing
            if (songInfo == null)
            {
                Discord.ClearPresence();
                return;
            }

            var songTitleAndArtist = Tidal.ExtractSongAndArtist(songInfo);

            var presence = new RichPresence()
            {
                Details = songTitleAndArtist.Item1,
                State = songTitleAndArtist.Item2,
                Assets = new Assets
                {
                    LargeImageKey = LARGEIMAGEKEY,
                    LargeImageText = LARGEIMAGETEXT
                }
            };

            Discord.SetPresence(presence);
        }

        public void Start()
        {
            if (!Timer.Enabled)
                Timer.Start();
        }

        public void Stop()
        {
            if (Timer.Enabled)
                Timer.Stop();

            Discord.ClearPresence();
        }

        public void Dispose()
        {
            Stop();
            Discord.Dispose();
            Timer.Dispose();
        }
    }
}
