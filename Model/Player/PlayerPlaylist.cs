using WinSonic.Model.Api;

namespace WinSonic.Model.Player
{
    public class PlayerPlaylist
    {
        public List<Song> Songs { get; set; } = [];

        public int SongIndex { get; set; } = -1;

        private PlayerPlaylist() { }

        public static PlayerPlaylist Instance { get; private set; } = new();

        public delegate void OnSongAddedEventHandler(object? sender, Song song, int index);
        public event OnSongAddedEventHandler? SongAdded;

        public delegate void OnSongRemovedEventHandler(object? sender, int index);
        public event OnSongRemovedEventHandler? SongRemoved;

        public delegate void OnSongIndexChangedEventHandler(object? sender, int index);
        public event OnSongIndexChangedEventHandler? SongIndexChanged;

        public void AddSong(Song song)
        {
            Songs.Add(song);
            SongAdded?.Invoke(this, song, Songs.Count - 1);
        }

        public void AddSong(Song song, int index)
        {
            Songs.Insert(index, song);
            SongAdded?.Invoke(this, song, index);
        }

        public void AddNextSong(Song song)
        {
            Songs.Insert(SongIndex + 1, song);
            SongAdded?.Invoke(this, song, SongIndex + 1);
        }

        public bool RemoveSong(int index)
        {
            if (Songs.Count > index)
            {
                Songs.RemoveAt(index);
                SongRemoved?.Invoke(this, index);
                if (SongIndex == index)
                {
                    SongIndex--;
                }
                return true;
            }
            return false;
        }

        public void ClearSongs()
        {
            for (int i = Songs.Count - 1; i >= 0; i--)
            {
                RemoveSong(i);
            }
            SongIndex = -1;
        }

        public void PlaySong(int index)
        {
            SongIndex = index;
            SongIndexChanged?.Invoke(this, index);
        }
    }
}
