using System.Collections.Generic;

namespace nowplaying_for_tidal.Data
{
    public static class Constants
    {
        // Discord RPC
        public const string DiscordRpcDefaultAppId = "735159392554713099";
        public const string DiscordRpcDefaultLargeImageKey = "tidal";
        public const string DiscordRpcDefaultLargeImageText = "TIDAL";
        public const int DiscordRpcTimecodeDiff = 1000;
        
        // Discord Assets
        public const int DiscordAssetsLimit = 300;
        public const int DiscordAssetsTimeUntilAvailability = 10; // due to caching, it takes rougly 10 min until uploaded assets are available for usage
        public static readonly HashSet<string> DiscordWhitelistedAssets = new() { "tidal" };
    }
}