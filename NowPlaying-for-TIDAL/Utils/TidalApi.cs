using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SimpleTidalApi;
using SimpleTidalApi.Model;

namespace nowplaying_for_tidal.Utils
{
    public static class TidalApi
    {
        /// <returns>Null if something went wrong or track couldn't be found</returns>
        public static Task<Track> QueryTrack(string title, string artists)
        {
            return Task.Run(async () =>
            {
                var key = TidalClient.GetAccessTokenFromTidalDesktop();

                if (!string.IsNullOrEmpty(key.Item1))
                {
                    Trace.TraceError("TidalApi: Could not get the key from TIDAL desktop for the following Reason: " +
                                     key.Item1);
                    return null;
                }

                if (key.Item2 == null)
                    return null;

                var loginKey = key.Item2;

                var searchResult = await TidalClient.Search(loginKey, title + " " + artists, eType: QueryFilter.TRACK);

                if (searchResult.Item1 != null)
                {
                    Trace.TraceError("TidalApi: Search query for the track failed for the following reason: " +
                                     searchResult.Item1);
                    return null;
                }

                if (searchResult.Item2?.Tracks == null || searchResult.Item2.Tracks.Count == 0)
                    return null;

                // Find correct track with matching artists in case of other songs having the same name, tracks are sorted by relevancy so the first match is usually correct.
                var artistsText = artists.Replace(", ", @" / ");
                return searchResult.Item2.Tracks.FirstOrDefault(t => t.ArtistsName == artistsText);
            });
        }
    }
}