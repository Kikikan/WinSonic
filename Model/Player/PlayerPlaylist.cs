﻿using WinSonic.Model.Api;

namespace WinSonic.Model.Player
{
    public class PlayerPlaylist
    {
        public List<Song> Songs { get; set; } = [];

        private PlayerPlaylist() { }

        public static PlayerPlaylist Instance { get; private set; } = new();

        public delegate void OnSongAddedEventHandler(object? sender, Song song, int index);
        public event OnSongAddedEventHandler? SongAdded;

        public delegate void OnSongRemovedEventHandler(object? sender, int index);
        public event OnSongRemovedEventHandler? SongRemoved;

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

        public void ClearSongs()
        {
            for (int i = Songs.Count - 1; i >= 0; i--)
            {
                RemoveSong(i);
            }
        }
    }
}
