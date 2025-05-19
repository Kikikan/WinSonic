using System;

namespace WinSonic.Model.Api
{
    public class Song : ApiObject
    {
        public string Title { get; private set; }
        public string Album { get; private set; }
        public string Artist { get; private set; }
        public Uri CoverImageUri { get; private set; }
        public Uri StreamUri { get; private set; }
        public int DiskNumber { get; private set; }
        public int Track { get; private set; }

        public Song(Child child, Server server) : base(child.Id, server)
        {
            Title = child.Title;
            Album = child.Album;
            Artist = child.Artist;
            DiskNumber = child.DiscNumber;
            Track = child.Track;
            CoverImageUri = new Uri($"{server.Address}/rest/getCoverArt{server.GetParameters()}&id={child.CoverArt}");
            StreamUri = new Uri($"{server.Address}/rest/stream{server.GetParameters()}&id={Id}");
        }
    }
}
