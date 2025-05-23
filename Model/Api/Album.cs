namespace WinSonic.Model.Api
{
    public class Album : ApiObject
    {
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public Uri CoverImageUrl { get; private set; }
        public bool IsFavourite { get; set; }

        public Album(string id, string title, string artist, bool isFavourite, Server server) : base(id, server)
        {
            Title = title;
            Artist = artist;
            IsFavourite = isFavourite;
            CoverImageUrl = new Uri($"{server.Address}/rest/getCoverArt{server.GetParameters()}&id={id}");
        }

        public Album(AlbumId3 album, Server server) : this(album.Id, album.Name, album.Artist, album.StarredSpecified, server)
        {

        }
    }
}
