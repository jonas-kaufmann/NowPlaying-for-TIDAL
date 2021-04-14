using System.Text;

namespace discord_rpc_tidal.Utils
{
    public class GeneralUtils
    {
        public static string CutDownStringToByteSize(string input, int maxLength)
        {
            var currentLength = Encoding.UTF8.GetByteCount(input);
            if (currentLength <= maxLength)
                return input;

            for (int endIndex = input.Length - 1; endIndex >= 0; endIndex--)
            {
                var cutString = input.Substring(0, endIndex + 1).Trim() + "..";
                if (Encoding.UTF8.GetByteCount(cutString) <= maxLength)
                {
                    return input.Substring(0, endIndex + 1);
                }
            }

            return string.Empty;
        }
    }
}