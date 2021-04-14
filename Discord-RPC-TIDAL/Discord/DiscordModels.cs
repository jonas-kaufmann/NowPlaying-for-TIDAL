using System;
using SimpleTidalApi.Model;

namespace discord_rpc_tidal.Discord
{
    public class DiscordAssetResponseEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
    }

    public class DiscordAsset
    {
        public string Name { get; set; }
        public DateTime Uploaded { get; set; }
        public DateTime LastUsed { get; set; }

    }
}