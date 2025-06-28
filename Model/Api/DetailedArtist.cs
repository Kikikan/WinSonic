namespace WinSonic.Model.Api
{
    public class DetailedArtist : ApiObject
    {
        public string Key { get; private set; }
        public string Name { get; private set; }
        public string Biography { get; private set; }
        public bool IsFavourite { get; set; }
        public Uri? SmallImageUri { get; private set; }
        public Uri? MediumImageUri { get; private set; }
        public Uri? LargeImageUri { get; private set; }

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
    }
}
