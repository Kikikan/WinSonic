namespace WinSonic.Model.Api
{
    public class DetailedArtist : ApiObject, IFavourite
    {
        public string Key { get; private set; }
        public string Name { get; private set; }
        public string Biography { get; private set; }
        public bool IsFavourite { get; set; }
        public Uri? SmallImageUri { get; private set; }
        public Uri? MediumImageUri { get; private set; }
        public Uri? LargeImageUri { get; private set; }

        public SubsonicApiHelper.StarType Type => SubsonicApiHelper.StarType.Artist;

        public DetailedArtist(Server server, string key, string id, string name, string biography, bool isFavourite, string smallImageUri, string mediumImageUri, string largeImageUri) : base(id, server)
        {
            Key = key;
            Name = name;
            Biography = biography;
            IsFavourite = isFavourite;
            SmallImageUri = !string.IsNullOrEmpty(smallImageUri) ? new Uri(smallImageUri) : null;
            MediumImageUri = !string.IsNullOrEmpty(mediumImageUri) ? new Uri(mediumImageUri) : null;
            LargeImageUri = !string.IsNullOrEmpty(largeImageUri) ? new Uri(largeImageUri) : null;
        }

        public DetailedArtist(Server server, ArtistId3 artist) : base(artist.Id, server)
        {
            Key = artist.Name[..1];
            Name = artist.Name;
            Biography = "";
            IsFavourite = artist.StarredSpecified;
            if (!string.IsNullOrEmpty(artist.ArtistImageUrl))
            {
                SmallImageUri = new Uri(artist.ArtistImageUrl);
                MediumImageUri = new Uri(artist.ArtistImageUrl);
                LargeImageUri = new Uri(artist.ArtistImageUrl);
            }
            else
            {
                SmallImageUri = null;
                MediumImageUri = null;
                LargeImageUri = null;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
