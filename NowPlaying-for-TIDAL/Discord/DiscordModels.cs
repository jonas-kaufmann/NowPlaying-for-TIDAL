using System;

namespace nowplaying_for_tidal.Discord
{
    public class DiscordAssetResponseEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
    }

    public class DiscordAsset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Uploaded { get; set; }
        public DateTime LastUsed { get; set; }

    }
}