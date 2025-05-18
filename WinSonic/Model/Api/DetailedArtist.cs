using System;

namespace WinSonic.Model.Api
{
    public class DetailedArtist
    {
        public Server Server { get; private set; }
        public string Key { get; private set; }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Biography { get; private set; }
        public Uri SmallImageUri { get; private set; }
        public Uri MediumImageUri { get; private set; }
        public Uri LargeImageUri { get; private set; }

        public DetailedArtist(Server server, string key, string id, string name, string biography, string smallImageUri, string mediumImageUri, string largeImageUri)
        {
            Server = server;
            Key = key;
            Id = id;
            Name = name;
            Biography = biography;
            SmallImageUri = smallImageUri != null ? new Uri(smallImageUri) : null;
            MediumImageUri = mediumImageUri != null ? new Uri(mediumImageUri) : null;
            LargeImageUri = largeImageUri != null ? new Uri(largeImageUri) : null;
        }
    }
}
