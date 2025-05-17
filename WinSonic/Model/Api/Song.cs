using System;

namespace WinSonic.Model.Api
{
    class Song
    {
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Album { get; private set; }
        public string Artist { get; private set; }
        public Uri CoverImageUri { get; private set; }
        public Uri StreamUri { get; private set; }

        public Song(string id, string title, string album, string artist, string coverId, Server server)
        {
            Id = id;
            Title = title;
            Album = album;
            Artist = artist;
            CoverImageUri = new Uri($"{server.Address}/rest/getCoverArt{server.GetParameters()}&id={coverId}");
            StreamUri = new Uri($"{server.Address}/rest/stream{server.GetParameters()}&id={Id}");
        }
    }
}
