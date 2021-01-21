using System.Diagnostics;

namespace discord_rpc_tidal
{
    class Tidal
    {
        private const string PROCESSNAME = "TIDAL";
        private const string SPLITSTRING = "-";

        /// <returns>all available info about the currently playing song or null if nothing is playing</returns>
        public static string GetCurrentlyPlaying()
        {
            string currentlyPlaying = null;
            foreach (var p in Process.GetProcessesByName(PROCESSNAME))
            {
                if (!string.IsNullOrWhiteSpace(p.MainWindowTitle) && !p.MainWindowTitle.Contains(PROCESSNAME, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    currentlyPlaying = p.MainWindowTitle;
                    break;
                }
            }

            return currentlyPlaying;
        }


        public static (string, string) ExtractSongAndArtist(string songInfo)
        {
            string songTitle = string.Empty;
            string artist = string.Empty;

            var cut = songInfo.Split(SPLITSTRING);

            return (cut[0].Trim(), cut[1].Trim());
        }

    }
}
