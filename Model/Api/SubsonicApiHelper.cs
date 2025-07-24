using System.Xml.Serialization;

namespace WinSonic.Model.Api
{
    public class SubsonicApiHelper
    {
        private static readonly int URL_LIMIT = 2000;
        private SubsonicApiHelper() { }

        public static Task<Response> Ping(Server server)
        {
            return Execute(server, "/rest/ping", []);
        }

        public static async Task<List<Album>> GetAlbumList(Server server, AlbumListType type, int size = 10, int offset = 0, int fromYear = -1, int toYear = -1, string genre = "")
        {
            List<(string, string)> parameters = [("type", type.ToString()), ("size", size.ToString()), ("offset", offset.ToString())];
            if (type == AlbumListType.byYear)
            {
                parameters.AddRange([("fromYear", fromYear.ToString()), ("toYear", toYear.ToString())]);
            }
            else if (type == AlbumListType.byGenre)
            {
                parameters.Add(("genre", genre));
            }
            var response = await Execute(server, "/rest/getAlbumList2", parameters);
            var albums = new List<Album>();
            if (response != null && response.AlbumList2 != null && response.AlbumList2.Count > 0)
            {
                foreach (var album in response.AlbumList2)
                {
                    albums.Add(new Album(album, server));
                }
            }
            return albums;
        }

        public static async Task<(List<ArtistId3>, List<Album>, List<Song>)> GetStarred(Server server)
        {
            var artists = new List<ArtistId3>();
            var albums = new List<Album>();
            var songs = new List<Song>();
            var response = await Execute(server, "/rest/getStarred2", []);
            if (response != null && response.Starred2 != null)
            {
                artists.AddRange(response.Starred2.Artist);
                albums.AddRange([.. response.Starred2.Album.Select(c => new Album(c, server))]);
                songs.AddRange([.. response.Starred2.Song.Select(c => new Song(c, server))]);
            }
            return (artists, albums, songs);
        }

        public static async Task<ArtistInfo2> GetArtistInfo(Server server, string id)
        {
            var response = await Execute(server, "/rest/getArtistInfo2", [("id", id)]);
            return response.ArtistInfo2;
        }

        public static async Task<List<DetailedArtist>> GetArtists(Server server)
        {
            var rs = await Execute(server, "/rest/getArtists", []);
            var artists = new List<DetailedArtist>();
            if (rs != null && rs.Artists != null)
            {
                foreach (var index in rs.Artists.Index)
                {
                    foreach (var artist in index.Artist)
                    {
                        var artistRs = await GetArtistInfo(server, artist.Id);
                        artists.Add(new DetailedArtist(server, index.Name, artist.Id, artist.Name, artistRs.Biography, artist.StarredSpecified, artistRs.SmallImageUrl, artistRs.MediumImageUrl, artistRs.LargeImageUrl));
                    }
                }
            }
            return artists;
        }

        public static async Task<ArtistWithAlbumsId3> GetArtist(Server server, string id)
        {
            var rs = await Execute(server, "/rest/getArtist", [("id", id)]);
            return rs.Artist;
        }

        public static async Task<AlbumInfo> GetAlbumInfo(Server server, string id)
        {
            var rs = await Execute(server, "/rest/getAlbumInfo2", [("id", id)]);
            return rs.AlbumInfo;
        }

        public static async Task<AlbumWithSongsId3> GetAlbum(Server server, string id)
        {
            var rs = await Execute(server, "/rest/getAlbum", [("id", id)]);
            return rs.Album;
        }

        public static async Task<bool> Scrobble(Server server, string id)
        {
            var rs = await Execute(server, "/rest/scrobble", [("id", id), ("submission", "false")]);
            return rs.Status == ResponseStatus.Ok;
        }

        public static async Task<bool> Star(Server server, bool star, StarType type, string id)
        {
            string paramName = type switch
            {
                StarType.Artist => "artistId",
                StarType.Album => "albumId",
                _ => "id",
            };
            var rs = await Execute(server, "/rest/" + (!star ? "un" : "") + "star", [(paramName, id)]);
            return rs.Status == ResponseStatus.Ok;
        }

        public static async Task<List<Song>> Search(Server server, int songCount = 9999, int songOffset = 0)
        {
            var rs = await Execute(server, "/rest/search3", [("query", ""), ("songCount", songCount.ToString()), ("songOffset", songOffset.ToString())]);
            return [.. rs.SearchResult3.Song.Select(s => new Song(s, server))];
        }

        public static async Task<List<Playlist>> GetPlaylists(Server server)
        {
            var rs = await Execute(server, "/rest/getPlaylists", []);
            return [.. rs.Playlists];
        }

        public static async Task<DetailedPlaylist> GetPlaylist(Server server, string id)
        {
            var rs = await Execute(server, "/rest/getPlaylist", [("id", id)]);

            if (rs.Playlist != null)
            {
                var playlist = rs.Playlist;
                List<Song> songs = [];
                foreach (var song in playlist.Entry)
                {
                    songs.Add(new Song(song, server));
                }
                return new DetailedPlaylist(server, playlist.Id, playlist.Name, playlist.Comment, playlist.Owner, playlist.Public, songs);
            }
            else
            {
                throw new ArgumentException($"Playlist with id '{id}' was not found.");
            }
        }

        public static async Task<Playlist> CreatePlaylist(Server server, string name, List<string> songIds)
        {
            return (await Execute(server, "/rest/createPlaylist", [("name", name), .. songIds.Select(s => ("songId", s))])).Playlist;
        }

        public static async Task UpdatePlaylist(DetailedPlaylist playlist)
        {
            DetailedPlaylist currPlaylist = await GetPlaylist(playlist.Server, playlist.Id);
            List<string> songIndices = [];
            for (int i = currPlaylist.Songs.Count - 1; i >= 0; i--)
            {
                songIndices.Add(i.ToString());
            }
            await UpdatePlaylist(playlist.Server, playlist.Id, playlist.Name, playlist.Description, playlist.IsPublic, songIndices, [.. playlist.Songs.Select(s => s.Id)]);
        }

        public static async Task UpdatePlaylist(Server server, string playlistId, string? name, string? comment, bool? isPublic, List<string> songIndicesToRemove, List<string> songIdsToAdd)
        {
            List<(string, string)> parameters = [];
            if (name != null)
            {
                parameters.Add(("name", name));
            }
            if (comment != null)
            {
                parameters.Add(("comment", comment));
            }
            if (isPublic != null)
            {
                parameters.Add(("public", ((bool)isPublic ? "true" : "false")));
            }
            parameters.AddRange([.. songIndicesToRemove.Select(index => ("songIndexToRemove", index))]);
            parameters.AddRange([.. songIdsToAdd.Select(index => ("songIdToAdd", index))]);
            await ExecuteLongUrl(server, "/rest/updatePlaylist", [("playlistId", playlistId)], parameters);
        }

        public static async Task DeletePlaylist(Server server, string id)
        {
            await Execute(server, "/rest/deletePlaylist", [("id", id)]);
        }

        private static async Task<Response> Execute(Server server, string baseUrl, List<(string, string)> parameters)
        {
            var urls = GetUrls(baseUrl, server.GetParameters(), parameters);
            if (urls.Count > 1)
            {
                string urlStrings = string.Join(";", urls);
                throw new ArgumentException($"Url must be less than or equal to {URL_LIMIT} characters. '{urlStrings}'");
            }
            else
            {
                return await Execute(server, urls[0]);
            }
        }

        private static async Task<List<Response>> ExecuteLongUrl(Server server, string baseUrl, List<(string, string)> requiredParameters,
                                                                 List<(string, string)> optionalParameters)
        {
            requiredParameters.AddRange(server.GetParameters());
            var urls = GetUrls(baseUrl, requiredParameters, optionalParameters);
            if (urls.Count > 1)
            {
                var tasks = urls.Select(url => Execute(server, url));
                return [.. await Task.WhenAll(tasks)];
            }
            else
            {
                return [await Execute(server, urls[0])];
            }
        }

        private static List<string> GetUrls(string baseUrl, List<(string, string)> requiredParameters, List<(string, string)> optionalParameters)
        {
            string baseUrlWithRequiredParams = $"{baseUrl}?";
            foreach (var parameter in requiredParameters)
            {
                baseUrlWithRequiredParams += GetParameterString(parameter) + "&";
            }
            baseUrlWithRequiredParams = baseUrlWithRequiredParams[..baseUrlWithRequiredParams.LastIndexOf('&')];

            List<string> urls = [];
            string currentUrl = $"{baseUrlWithRequiredParams}";
            foreach (var parameter in optionalParameters)
            {
                string newUrl = $"{currentUrl}&{GetParameterString(parameter)}";
                if (newUrl.Length >= URL_LIMIT)
                {
                    urls.Add(currentUrl);
                    currentUrl = $"{baseUrlWithRequiredParams}&{GetParameterString(parameter)}";
                }
                else
                {
                    currentUrl = newUrl;
                }
            }
            urls.Add(currentUrl);
            return urls;
        }

        internal static string GetParameterString((string, string) parameter)
        {
            return $"{parameter.Item1}={parameter.Item2}";
        }

        private static async Task<Response> Execute(Server server, string url)
        {
            try
            {
                using HttpResponseMessage response = await server.Client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string rs = await response.Content.ReadAsStringAsync();
                return DeserializeXml(rs);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException("Network error while fetching data.", ex, server);
            }
            catch (TaskCanceledException ex)
            {
                throw new ApiException("Request timed out.", ex, server);
            }
            catch (Exception ex)
            {
                throw new ApiException("Unexpected error occurred.", ex, server);
            }
        }

        private static Response DeserializeXml(string xml)
        {
            var serializer = new XmlSerializer(typeof(Response));
            using var reader = new StringReader(xml);
            if (serializer.Deserialize(reader) is Response response)
            {
                return response;
            }
            throw new IllegalResponseException();
        }

        public enum AlbumListType
        {
            random, newest, highest, frequent, recent, alphabeticalByName, alphabeticalByArtist, starred, byYear, byGenre
        }

        public enum StarType
        {
            Song, Album, Artist
        }
    }
}
