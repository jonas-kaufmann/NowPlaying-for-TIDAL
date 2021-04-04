using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace discord_rpc_tidal
{
    class Util
    {
        public static String ConvertImageURLToBase64(String url)
        {
            StringBuilder _sb = new StringBuilder();

            Byte[] _byte = GetImage(url);

            _sb.Append(Convert.ToBase64String(_byte, 0, _byte.Length));

            return _sb.ToString();
        }

        private static byte[] GetImage(string url)
        {
            Stream stream = null;
            byte[] buf;

            try
            {
                WebProxy myProxy = new WebProxy();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                stream = response.GetResponseStream();

                using (BinaryReader br = new BinaryReader(stream))
                {
                    int len = (int)(response.ContentLength);
                    buf = br.ReadBytes(len);
                    br.Close();
                }

                stream.Close();
                response.Close();
            }
            catch (Exception exp)
            {
                buf = null;
            }

            return (buf);
        }

        public static String AssureByteSize(String input, Int32 maxLength)
        {
            for (Int32 i = input.Length - 1; i >= 0; i--)
            {
                if (Encoding.UTF8.GetByteCount(input.Substring(0, i + 1)) <= maxLength)
                {
                    return input.Substring(0, i + 1);
                }
            }

            return String.Empty;
        }

        public static string SanitizeAlbumName(string albumName)
        {
            char[] albumArray = albumName.ToCharArray();

            Regex symbolPattern = new Regex(@"[!@#$%^&*()+=\'[\""{\]};:<>|./?,\s-]", RegexOptions.Compiled);

            foreach (Match m in symbolPattern.Matches(albumName))
                albumArray[m.Index] = '_';

            string newAlbum = new String(albumArray).ToLower();

            return newAlbum;
        }

        public static void ShowUploadToast(string albumName, bool wasSuccessful)
        {
            var toast = new ToastContentBuilder()
                .AddText(wasSuccessful ? "Uploaded Artwork" : "Failed Upload")
                .AddText(albumName);

            toast.Show();
        }
    }

    public class DiscordUtil
    {
        public static HttpContent GetAssetList()
        {
            var response = DiscordRPC.httpClient.GetAsync($"https://discordapp.com/api/oauth2/applications/{DiscordRPC.APPID}/assets", HttpCompletionOption.ResponseContentRead);

            return response.Result.Content;
        }

        public async static Task UploadAsset(string album_name, string image_data)
        {
            string payload = JsonConvert.SerializeObject(new
            {
                name = album_name,
                type = "1",
                image = image_data,
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await DiscordRPC.httpClient.PostAsync($"https://discordapp.com/api/oauth2/applications/{DiscordRPC.APPID}/assets", content);

            var response_content = await response.Content.ReadAsStringAsync();

            var wasSuccessful = response_content.Contains(album_name);

            Util.ShowUploadToast(album_name, wasSuccessful);
        }
    }
}
