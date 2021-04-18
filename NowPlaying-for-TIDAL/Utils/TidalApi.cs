using System;
using System.Collections.Generic;
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

                // Find correct track
                var artistsText = artists.Replace(", ", @" / ");
                var tracksWithCorrectArtists =
                    searchResult.Item2.Tracks.Where(t => t.ArtistsName == artistsText); // match artist names
                var tracksWithCorrectName =
                    tracksWithCorrectArtists.Where(t => t.DisplayTitle.StartsWith(title)).OrderBy(t => t.DisplayTitle.Length).ToList(); // match display title

                // take songs with display title closest to input
                var firstTrack = tracksWithCorrectName.FirstOrDefault();
                if (firstTrack == null)
                    return null;
                tracksWithCorrectName = tracksWithCorrectName.Where(t => t.DisplayTitle.Length == firstTrack.DisplayTitle.Length).ToList(); // eliminate all tracks that have a longer name than the shortest
                if (tracksWithCorrectName.Count == 1)
                    return tracksWithCorrectName.FirstOrDefault();
                
                // Find the track within an album with the most tracks
                // Query API for album information
                var albums = new Dictionary<long, Album>();
                Parallel.ForEach(tracksWithCorrectName, track =>
                {
                    var task = TidalClient.Search(loginKey, track.Album.Title + " " + track.Artists[0].Name, eType: QueryFilter.ALBUM);
                    task.Wait();
                    var (item1, item2) = task.Result;
                    if (item1 != null || item2.Albums.Count == 0)
                        return;

                    var correspondingAlbum = item2.Albums.FirstOrDefault(album => album.ID == track.Album.ID);
                    if (correspondingAlbum == null) return;
                    lock (albums)
                    {
                        albums.Add(correspondingAlbum.ID, correspondingAlbum);
                    }
                });
                
                Console.WriteLine(albums.Count);

                foreach (var track in tracksWithCorrectName.Where(track => albums.ContainsKey(track.Album.ID)))
                {
                    track.Album = albums[track.Album.ID];
                }
                
                // return title within the album with the most tracks
                return tracksWithCorrectName.OrderByDescending(t => t.Album.NumberOfTracks).FirstOrDefault();

            });
        }
    }
}