using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using discord_rpc_tidal.Data;
using discord_rpc_tidal.Utils;
using SimpleTidalApi.Model;

namespace discord_rpc_tidal.Discord
{
    public static class AssetManager
    {
        private static readonly DiscordApi DiscordApi = new();
        private static Dictionary<string, DiscordAsset> _assets = new();

        /// <returns>True if succesful or already exists</returns>
        public static async Task<bool> UploadIfNotExists(Album album)
        {
            if (Exists(album))
                return true;

            if (!AppConfig.UseAlbumArtwork)
                return false;

            var imageData = HttpUtils.ConvertImageToBase64(album.CoverMidUrl);

            if (imageData == null)
                return false;

            if (!await DiscordApi.UploadAsset(album.ID.ToString(), imageData))
                return false;

            lock (_assets)
            {
                _assets.Add(album.ID.ToString(), new DiscordAsset
                {
                    Name = album.ID.ToString(),
                    Uploaded = DateTime.Now,
                    LastUsed = DateTime.Now
                });

                return true;
            }
        }

        public static bool Exists(Album album)
        {
            if (!AppConfig.UseAlbumArtwork)
                return false;
            
            lock (_assets)
            {
                return _assets.ContainsKey(album.ID.ToString());
            }
        }

        /// <summary>
        /// It takes some time before uploaded assets are available to be used. Therefore this method checks whether the asset has been uploaded and that some time has passed since then
        /// </summary>
        /// <param name="album"></param>
        /// <returns></returns>
        public static bool Available(Album album)
        {
            if (!AppConfig.UseAlbumArtwork)
                return false;
            
            lock (_assets)
            {
                return Exists(album) &&
                       DateTime.Now.Subtract(_assets[album.ID.ToString()].Uploaded) >=
                       TimeSpan.FromMinutes(Constants.DiscordAssetsTimeUntilAvailability);
            }
        }

        public static void NotifyUsed(Album album)
        {
            if (!AppConfig.UseAlbumArtwork)
                return;
            
            lock (_assets)
            {
                if (_assets.ContainsKey(album.ID.ToString()))
                {
                    _assets[album.ID.ToString()].LastUsed = DateTime.Now;
                }
            }
        }

        public static async void Sync()
        {
            if (!AppConfig.UseAlbumArtwork)
                return;
            
            var assetList = await DiscordApi.GetAssetList();
            if (assetList == null)
                return;

            var newAssets = new Dictionary<string, DiscordAsset>();

            lock (_assets)
            {
                foreach (var asset in assetList)
                {
                    if (_assets.ContainsKey(asset.Name))
                    {
                        newAssets.Add(asset.Name, _assets[asset.Name]);
                    }
                    else if (
                        !Constants.DiscordWhitelistedAssets
                            .Contains(asset
                                .Name)) // if asset is unknown and not whitelisted, mark it to be deleted by setting LastUsed to DateTime.Min
                    {
                        newAssets.Add(asset.Name, new DiscordAsset
                        {
                            Name = asset.Name,
                            Uploaded = DateTime.MinValue,
                            LastUsed = DateTime.MinValue
                        });
                    }
                }
            }
            
            lock (_assets)
            {
                _assets = newAssets;
            }

            // upload default asset
            if (assetList.FirstOrDefault(a => a.Name == Constants.DiscordRpcDefaultLargeImageKey) != null)
                return;

            var imageRaw = await File.ReadAllBytesAsync("Resources/TIDAL Logo.png");
            var encodedImage = HttpUtils.ConvertImageToBase64(imageRaw);
            await DiscordApi.UploadAsset(Constants.DiscordRpcDefaultLargeImageKey, encodedImage);
        }
    }
}