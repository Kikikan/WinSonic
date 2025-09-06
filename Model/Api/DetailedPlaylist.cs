namespace WinSonic.Model.Api
{
    public class DetailedPlaylist : ApiObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; private set; }
        public bool IsPublic { get; set; }
        public List<Song> Songs { get; private set; }

        public DetailedPlaylist(Server server, string id, string name, string description, string owner, bool isPublic, List<Song> songs) : base(id, server)
        {
            Name = name;
            Description = description;
            Owner = owner;
            IsPublic = isPublic;
            Songs = songs;
        }

        public DetailedPlaylist(DetailedPlaylist playlist) : base(playlist.Id, playlist.Server)
        {
            Name = playlist.Name;
            Description = playlist.Description;
            Owner = playlist.Owner;
            IsPublic = playlist.IsPublic;
            Songs = [.. playlist.Songs];
        }

        public void CopyValuesFrom(DetailedPlaylist playlist)
        {
            Name = playlist.Name;
            Description = playlist.Description;
            Owner = playlist.Owner;
            IsPublic = playlist.IsPublic;
            Songs = [.. playlist.Songs];
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
