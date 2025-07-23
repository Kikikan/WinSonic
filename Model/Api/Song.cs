namespace WinSonic.Model.Api
{
    public class Song : ApiObject, IFavourite
    {
        public string Title { get; private set; }
        public string Album { get; private set; }
        public string Artist { get; private set; }
        public Uri CoverImageUri { get; private set; }
        public Uri StreamUri { get; private set; }
        public int DiskNumber { get; private set; }
        public int Track { get; private set; }
        public bool IsFavourite { get; set; }
        public int Duration { get; private set; }
        public SubsonicApiHelper.StarType Type => SubsonicApiHelper.StarType.Song;

        public Song(Child child, Server server) : base(child.Id, server)
        {
            Title = child.Title;
            Album = child.Album;
            Artist = child.Artist;
            DiskNumber = child.DiscNumber;
            Track = child.Track;
            IsFavourite = child.StarredSpecified;
            Duration = child.Duration;
            CoverImageUri = new Uri($"{server.Address}/rest/getCoverArt{server.GetStringParameters()}&id={child.CoverArt}");
            StreamUri = new Uri($"{server.Address}/rest/stream{server.GetStringParameters()}&id={Id}");
        }
    }
}
