using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSonic.Model.Api;

namespace WinSonic.Model.Player
{
    public class PlayerPlaylist
    {
        private int _songIndex;
        public int SongIndex { get => _songIndex; set { int oldIndex = _songIndex; _songIndex = value; SongIndexChanged?.Invoke(this, oldIndex); } }
        public List<Song> Songs { get; set; } = [];

        private PlayerPlaylist() { }

        public static PlayerPlaylist Instance { get; private set; } = new();
        
        public delegate void OnSongAddedEventHandler(object? sender, Song song);
        public event OnSongAddedEventHandler SongAdded;

        public delegate void OnSongRemovedEventHandler(object? sender, int index);
        public event OnSongRemovedEventHandler SongRemoved;

        public delegate void OnSongIndexChangedEventHandler(object? sender, int oldIndex);
        public event OnSongIndexChangedEventHandler SongIndexChanged;

        public Song? Song { get { return SongIndex >= Songs.Count ? null : Songs[SongIndex]; } }

        public void AddSong(Song song)
        {
            Songs.Add(song);
            SongAdded?.Invoke(this, song);
        }

        public bool RemoveSong(int index)
        {
            if (Songs.Count > index)
            {
                Songs.RemoveAt(index);
                SongRemoved?.Invoke(this, index);
                // TODO: changing songIndex if index <= songIndex
                return true;
            }
            return false;
        }
    }
}
