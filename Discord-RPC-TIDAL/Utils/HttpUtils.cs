using System;
using System.IO;
using System.Net;

namespace discord_rpc_tidal.Utils
{
    public class HttpUtils
    {
        public static string ConvertImageToBase64(string urlToImage)
        {
            var imageRaw = GetImage(urlToImage);

            return imageRaw == null ? null : Convert.ToBase64String(imageRaw, 0, imageRaw.Length);
        }
        
        public static string ConvertImageToBase64(byte[] rawImage)
        {
            return Convert.ToBase64String(rawImage, 0, rawImage.Length);
        }


        private static byte[] GetImage(string url)
        {
            byte[] buf;

            try
            {
                var req = (HttpWebRequest)WebRequest.Create(url);

                var response = (HttpWebResponse)req.GetResponse();
                var stream = response.GetResponseStream();

                using (BinaryReader br = new BinaryReader(stream))
                {
                    int len = (int)(response.ContentLength);
                    buf = br.ReadBytes(len);
                    br.Close();
                }

                stream.Close();
                response.Close();
            }
            catch (Exception)
            {
                buf = null;
            }

            return buf;
        }
    }
}