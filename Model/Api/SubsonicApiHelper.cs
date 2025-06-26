using System.Xml.Serialization;
using WinSonic.Model.Util;

namespace WinSonic.Model.Api
{
    public class SubsonicApiHelper
    {
        private SubsonicApiHelper() { }

        public static Task<Response> Ping(Server server)
        {
            return Execute(server, $"/rest/ping{server.GetParameters()}");
        }

        public static async Task<List<Album>> GetAlbumList(Server server, AlbumListType type, int size = 10, int offset = 0, int fromYear = -1, int toYear = -1, string genre = "")
        {
            string parameters = $"/rest/getAlbumList2{server.GetParameters()}&type={type}&size={size}&offset={offset}";
            if (type == AlbumListType.byYear)
            {
                parameters += $"&fromYear={fromYear}&toYear={toYear}";
            }
            else if (type == AlbumListType.byGenre)
            {
                parameters += $"&genre={genre}";
            }
            var response = await Execute(server, parameters);
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

        public static async Task<Trio<List<ArtistId3>, List<Album>, List<Song>>> GetStarred(Server server)
        {
            var artists = new List<ArtistId3>();
            var albums = new List<Album>();
            var songs = new List<Song>();
            string parameters = $"/rest/getStarred2{server.GetParameters()}";
            var response = await Execute(server, parameters);
            if (response != null && response.Starred2 != null)
            {
                artists.AddRange(response.Starred2.Artist);
                albums.AddRange([.. response.Starred2.Album.Select(c => new Album(c, server))]);
                songs.AddRange([.. response.Starred2.Song.Select(c => new Song(c, server))]);
            }
            return new Trio<List<ArtistId3>, List<Album>, List<Song>>(artists, albums, songs);
        }

        public static async Task<ArtistInfo2> GetArtistInfo(Server server, string id)
        {
            string parameters = $"/rest/getArtistInfo2{server.GetParameters()}&id={id}";
            var response = await Execute(server, parameters);
            return response.ArtistInfo2;
        }

        public static async Task<List<DetailedArtist>> GetArtists(Server server)
        {
            var rs = await Execute(server, $"/rest/getArtists{server.GetParameters()}");
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
            var rs = await Execute(server, $"/rest/getArtist{server.GetParameters()}&id={id}");
            return rs.Artist;
        }

        public static async Task<AlbumInfo> GetAlbumInfo(Server server, string id)
        {
            var rs = await Execute(server, $"/rest/getAlbumInfo2{server.GetParameters()}&id={id}");
            return rs.AlbumInfo;
        }

        public static async Task<AlbumWithSongsId3> GetAlbum(Server server, string id)
        {
            var rs = await Execute(server, $"/rest/getAlbum{server.GetParameters()}&id={id}");
            return rs.Album;
        }

        public static async Task<bool> Scrobble(Server server, string id)
        {
            var rs = await Execute(server, $"/rest/scrobble{server.GetParameters()}&id={id}&submission=false");
            return rs.Status == ResponseStatus.Ok;
        }

        public static async Task<bool> Star(Server server, bool star, StarType type, string id)
        {
            string url = "/rest/" + (!star ? "un" : "") + $"star{server.GetParameters()}&";
            url += type switch
            {
                StarType.Artist => "artistId",
                StarType.Album => "albumId",
                _ => "id",
            };
            url += $"={id}";
            var rs = await Execute(server, url);
            return rs.Status == ResponseStatus.Ok;
        }

        public static async Task<List<Song>> Search(Server server, int songCount = 9999, int songOffset = 0)
        {
            var rs = await Execute(server, $"/rest/search3{server.GetParameters()}&query=&songCount={songCount}&songOffset={songOffset}");
            return [.. rs.SearchResult3.Song.Select(s => new Song(s, server))];
        }

        public static async Task<List<Playlist>> GetPlaylists(Server server)
        {
            var rs = await Execute(server, $"/rest/getPlaylists{server.GetParameters()}");
            return [.. rs.Playlists];
        }

        public static async Task<PlaylistWithSongs> GetPlaylist(Server server, string id)
        {
            var rs = await Execute(server, $"/rest/getPlaylist{server.GetParameters()}&id={id}");
            return rs.Playlist;
        }

        public static async Task<Playlist> CreatePlaylist(Server server, string name, string songId)
        {
            var rs = await Execute(server, $"/rest/createPlaylist{server.GetParameters()}&name={name}&songId={songId}");
            return rs.Playlist;
        }

        public static async Task UpdatePlaylist(Server server, string playlistId, string songId, bool isAdd)
        {
            var url = $"/rest/updatePlaylist{server.GetParameters()}&playlistId={playlistId}&";
            if (isAdd)
            {
                url += $"songIdToAdd={songId}";
            }
            else
            {
                url += $"songIndexToRemove={songId}";
            }
            await Execute(server, url);
        }

        public static async Task DeletePlaylist(Server server, string id)
        {
            await Execute(server, $"/rest/deletePlaylist{server.GetParameters()}&id={id}");
        }

        private static async Task<Response> Execute(Server server, string url)
        {
            using HttpResponseMessage response = await server.Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string rs = await response.Content.ReadAsStringAsync();
            return DeserializeXml(rs);
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
