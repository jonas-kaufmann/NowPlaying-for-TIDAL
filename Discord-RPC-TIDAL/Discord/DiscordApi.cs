using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using discord_rpc_tidal.Data;

namespace discord_rpc_tidal.Discord
{
    public class DiscordApi
    {
        private readonly HttpClient HttpClient = new();

        public DiscordApi()
        {
            HttpClient.DefaultRequestHeaders.Add("Authorization", AppConfig.DiscordMfaToken);
        }

        /// <returns>Null if assets can't be retrieved</returns>
        public async Task<List<DiscordAssetResponseEntry>> GetAssetList()
        {
            var response = HttpClient.GetAsync(
                $"https://discordapp.com/api/oauth2/applications/{AppConfig.DiscordAppId}/assets",
                HttpCompletionOption.ResponseContentRead);

            var responseText = await response.Result.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseText))
                return null;

            List<DiscordAssetResponseEntry> assets = null;
            try
            {
                assets = JsonSerializer.Deserialize<List<DiscordAssetResponseEntry>>(responseText,
                    new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            }
            catch (JsonException)
            {
                Trace.TraceError("DiscordAPI: Can't retrieve list of assets. Reason: " + responseText);
            }

            return assets;
        }

        /// <param name="name">Name under which the asset should appear</param>
        /// <param name="imageData">image data encoded in Base64</param>
        /// <returns>Id of the uploaded asset if succesful</returns>
        public async Task<string> UploadAsset(string name, string imageData)
        {
            string payload = JsonSerializer.Serialize(new
            {
                name = name,
                type = "1",
                image = "data:image/png;base64," + imageData,
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync(
                $"https://discordapp.com/api/oauth2/applications/{AppConfig.DiscordAppId}/assets", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            DiscordAssetResponseEntry assetResponse = null;
            try
            {
                assetResponse = JsonSerializer.Deserialize<DiscordAssetResponseEntry>(responseContent,
                    new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            }
            catch (JsonException)
            {
                
            }

            // not succesful
            if (assetResponse == null)
            {
                Trace.TraceError($"DiscordAPI: Upload of asset {name} failed\nreason: {responseContent}");

                return null;
            }
            
            Trace.TraceInformation($"DiscordAPI: Upload of asset {name} was successful");
            return assetResponse.Id;
        }

        /// <returns>True if succesful</returns>
        public async Task<bool> DeleteAsset(string assetId)
        {
            var response =
                await HttpClient.DeleteAsync(
                    $"https://discordapp.com/api/oauth2/applications/{AppConfig.DiscordAppId}/assets/{assetId}");

            Trace.TraceInformation(response.IsSuccessStatusCode
                ? $"DiscordAPI: Deletion of asset with id {assetId} was successful"
                : $"DiscordAPI: Deletion of asset with id {assetId} failed");

            return response.IsSuccessStatusCode;
        }
    }
}