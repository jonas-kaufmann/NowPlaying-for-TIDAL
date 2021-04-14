using System;
using System.IO;
using System.Text.Json;

namespace discord_rpc_tidal.Data
{
    public static class AppConfig
    {
        public static readonly string ConfigPath =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{AppDomain.CurrentDomain.FriendlyName}\\config.json";

        private static readonly JsonSerializerOptions JsonOptions = new() {WriteIndented = true};

        private static ConfigModel _configData = new();

        public static string DiscordAppId
        {
            get
            {
                lock (_configData)
                {
                    return UseAlbumArtwork
                        ? _configData.DiscordAppId : Constants.DiscordRpcDefaultAppId;
                }
            }
            set
            {
                lock (_configData)
                {
                    _configData.DiscordAppId = value;
                }
            }
        }

        public static string DiscordMfaToken
        {
            get
            {
                lock (_configData)
                {
                    return _configData.DiscordMfaToken;
                }
            }
            set
            {
                lock (_configData)
                {
                    _configData.DiscordMfaToken = value;
                }
            }
        }

        public static bool UseAlbumArtwork
        {
            get
            {
                lock (_configData)
                {
                    return !string.IsNullOrWhiteSpace(_configData.DiscordAppId) &&
                           _configData.DiscordAppId != Constants.DiscordRpcDefaultAppId &&
                           !string.IsNullOrWhiteSpace(_configData.DiscordMfaToken);
                }
            }
        }


        public static void Load()
        {
            lock (_configData)
            {
                string json = null;
                try
                {
                    json = File.ReadAllText(ConfigPath);
                }
                catch (DirectoryNotFoundException)
                {
                }
                catch (FileNotFoundException)
                {
                }

                if (string.IsNullOrEmpty(json))
                {
                    _configData = new ConfigModel();
                    return;
                }

                _configData = JsonSerializer.Deserialize<ConfigModel>(json, JsonOptions);
            }
        }

        public static void Save()
        {
            lock (_configData)
            {
                var json = JsonSerializer.Serialize(_configData, JsonOptions);
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                File.WriteAllText(ConfigPath, json);
            }
        }
    }
}