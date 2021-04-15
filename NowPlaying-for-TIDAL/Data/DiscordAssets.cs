using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using nowplaying_for_tidal.Discord;

namespace nowplaying_for_tidal.Data
{
    public static class DiscordAssets
    {
        private static readonly string FilePath =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{AppDomain.CurrentDomain.FriendlyName}\\DiscordAssets.json";

        private static readonly JsonSerializerOptions JsonOptions = new() {WriteIndented = true};

        private static Dictionary<string, DiscordAsset> _assets = new();

        public static Dictionary<string, DiscordAsset> Assets
        {
            get
            {
                lock (_assets)
                {
                    return _assets;
                }
            }
            set
            {
                lock (_assets)
                {
                    _assets = value;
                }
            }
        }

        public static void Load()
        {
            lock (_assets)
            {
                string json = null;
                try
                {
                    json = File.ReadAllText(FilePath);
                }
                catch (DirectoryNotFoundException)
                {
                }
                catch (FileNotFoundException)
                {
                }

                if (string.IsNullOrEmpty(json))
                {
                    _assets = new();
                    return;
                }

                _assets = JsonSerializer.Deserialize<Dictionary<string, DiscordAsset>>(json, JsonOptions);
            }
        }

        public static void Save()
        {
            lock (_assets)
            {
                var json = JsonSerializer.Serialize(_assets, JsonOptions);
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, json);
            }
        }
    }
}