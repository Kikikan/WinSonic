using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WinSonic.Model.Util;

namespace WinSonic.Model.Api
{
    internal class SubsonicApiHelper
    {
        private SubsonicApiHelper() { }

        internal static Task<Response> Ping(Server server)
        {
            return Execute(server, $"/rest/ping{server.GetParameters()}");
        }

        internal static async Task<List<Album>> GetAlbumList(Server server, AlbumListType type, int size = 10, int offset = 0, int fromYear = -1, int toYear = -1, string genre = "")
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

        internal static async Task<Trio<List<ArtistId3>, List<Album>, List<Song>>> GetStarred(Server server)
        {
            var artists = new List<ArtistId3>();
            var albums = new List<Album>();
            var songs = new List<Song>();
            string parameters = $"/rest/getStarred2{server.GetParameters()}";
            var response = await Execute(server, parameters);
            if (response != null && response.Starred2 != null)
            {
                artists.AddRange(response.Starred2.Artist);
                albums.AddRange(response.Starred2.Album
                    .Select(c => new Album(c, server))
                    .ToImmutableList());
                songs.AddRange(response.Starred2.Song
                    .Select(c => new Song(c, server))
                    .ToImmutableList());
            }
            return new Trio<List<ArtistId3>, List<Album>, List<Song>>(artists, albums, songs);
        }

        internal static async Task<ArtistInfo2?> GetArtistInfo(Server server, string id)
        {
            string parameters = $"/rest/getArtistInfo2{server.GetParameters()}&id={id}";
            var response = await Execute(server, parameters);
            return response?.ArtistInfo2;
        }

        internal static async Task<List<DetailedArtist>> GetArtists(Server server)
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

        internal static async Task<ArtistWithAlbumsId3> GetArtist(Server server, string id)
        {
            var rs = await Execute(server, $"/rest/getArtist{server.GetParameters()}&id={id}");
            return rs.Artist;
        }

        internal static async Task<AlbumInfo> GetAlbumInfo(Server server, string id)
        {
            var rs = await Execute(server, $"/rest/getAlbumInfo2{server.GetParameters()}&id={id}");
            return rs.AlbumInfo;
        }

        internal static async Task<AlbumWithSongsId3> GetAlbum(Server server, string id)
        {
            var rs = await Execute(server, $"/rest/getAlbum{server.GetParameters()}&id={id}");
            return rs.Album;
        }

        private static async Task<Response> Execute(Server server, string url)
        {
            using (HttpResponseMessage response = await server.Client.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                string rs = await response.Content.ReadAsStringAsync();
                return DeserializeXml(rs);
            }
        }

        private static Response DeserializeXml(string xml)
        {
            var serializer = new XmlSerializer(typeof(Response));
            using (var reader = new StringReader(xml))
            {
                return (Response)serializer.Deserialize(reader);
            }
        }

        internal enum AlbumListType
        {
            random, newest, highest, frequent, recent, alphabeticalByName, alphabeticalByArtist, starred, byYear, byGenre
        }
    }
}
