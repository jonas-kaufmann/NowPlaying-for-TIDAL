using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using nowplaying_for_tidal.Data;
using nowplaying_for_tidal.Utils;
using SimpleTidalApi.Model;

namespace nowplaying_for_tidal.Discord
{
    public static class AssetManager
    {
        private static readonly DiscordApi DiscordApi = new();

        /// <returns>True if succesful or already exists</returns>
        public static async Task<bool> UploadIfNotExists(Album album)
        {
            if (!AppConfig.UseAlbumArtwork)
                return false;

            if (Exists(album))
                return true;


            // make room for the new asset
            IEnumerable<DiscordAsset> assetsToDelete = new List<DiscordAsset>(0);
            lock (DiscordAssets.Assets)
            {
                var numberOfAssetsToDelete = DiscordAssets.Assets.Count - Constants.DiscordAssetsLimit + 1;
                if (numberOfAssetsToDelete > 0)
                    assetsToDelete = DiscordAssets.Assets.Values.OrderBy(a => a.LastUsed).Take(numberOfAssetsToDelete);
            }

            foreach (var asset in assetsToDelete)
            {
                if (!await DiscordApi.DeleteAsset(asset.Id))
                    return false;

                DiscordAssets.Assets.Remove(asset.Name);
                DiscordAssets.Save();
            }

            
            // upload asset
            var imageData = HttpUtils.ConvertImageToBase64(album.CoverMidUrl);

            if (imageData == null)
                return false;

            var assetId = await DiscordApi.UploadAsset(album.ID.ToString(), imageData);
            if (assetId == null)
                return false;

            lock (DiscordAssets.Assets)
            {
                DiscordAssets.Assets.Add(album.ID.ToString(), new DiscordAsset
                {
                    Id = assetId,
                    Name = album.ID.ToString(),
                    Uploaded = DateTime.Now,
                    LastUsed = DateTime.Now
                });
                
                DiscordAssets.Save();

                return true;
            }
        }

        public static bool Exists(Album album)
        {
            if (!AppConfig.UseAlbumArtwork)
                return false;

            lock (DiscordAssets.Assets)
            {
                return DiscordAssets.Assets.ContainsKey(album.ID.ToString());
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

            lock (DiscordAssets.Assets)
            {
                return Exists(album) &&
                       DateTime.Now.Subtract(DiscordAssets.Assets[album.ID.ToString()].Uploaded) >=
                       TimeSpan.FromMinutes(Constants.DiscordAssetsTimeUntilAvailability);
            }
        }

        public static void NotifyUsed(Album album)
        {
            if (!AppConfig.UseAlbumArtwork)
                return;

            lock (DiscordAssets.Assets)
            {
                if (DiscordAssets.Assets.ContainsKey(album.ID.ToString()))
                {
                    DiscordAssets.Assets[album.ID.ToString()].LastUsed = DateTime.Now;
                    DiscordAssets.Save();
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

            lock (DiscordAssets.Assets)
            {
                foreach (var asset in assetList)
                {
                    // if asset already exists then delete this one
                    if (newAssets.ContainsKey(asset.Name))
                    {
                        _ = DiscordApi.DeleteAsset(asset.Id);
                        continue;
                    }

                    DiscordAsset assetToBeAdded;
                    if (DiscordAssets.Assets.ContainsKey(asset.Name))
                    {
                        assetToBeAdded = DiscordAssets.Assets[asset.Name];
                    }
                    // asset unknown
                    else
                    {
                        assetToBeAdded = new DiscordAsset
                        {
                            Id = asset.Id,
                            Name = asset.Name,
                            Uploaded = DateTime.MinValue,
                            LastUsed = DateTime.MinValue
                        };

                        if (Constants.DiscordWhitelistedAssets.Contains(asset.Name))
                            assetToBeAdded.LastUsed = DateTime.MaxValue;
                    }

                    newAssets.Add(asset.Name, assetToBeAdded);
                }
            }

            lock (DiscordAssets.Assets)
            {
                DiscordAssets.Assets = newAssets;
                DiscordAssets.Save();
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