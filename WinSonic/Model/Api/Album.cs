using System;

namespace WinSonic.Model.Api
{
    public class Album
    {
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public Server Server { get; private set; }
        public Uri CoverImageUrl { get; private set; }
        public bool IsFavourite { get; private set; }

        public Album(string id, string title, string artist, bool isFavourite, Server server)
        {
            Id = id;
            Title = title;
            Artist = artist;
            IsFavourite = isFavourite;
            Server = server;
            CoverImageUrl = new Uri($"{server.Address}/rest/getCoverArt{server.GetParameters()}&id={id}");
        }

        public Album(AlbumId3 album, Server server) : this(album.Id, album.Name, album.Artist, album.StarredSpecified, server)
        {

        }
    }
}
