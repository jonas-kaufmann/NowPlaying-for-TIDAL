using System.Text;

namespace nowplaying_for_tidal.Utils
{
    public class GeneralUtils
    {
        public static string CutDownStringToByteSize(string input, int maxLengthInBytes)
        {
            var currentLength = Encoding.UTF8.GetByteCount(input);
            if (currentLength <= maxLengthInBytes)
                return input;

            for (int endIndex = input.Length - 1; endIndex >= 0; endIndex--)
            {
                var cutString = input.Substring(0, endIndex + 1).Trim() + "..";
                if (Encoding.UTF8.GetByteCount(cutString) <= maxLengthInBytes)
                {
                    return cutString;
                }
            }

            return string.Empty;
        }
    }
}