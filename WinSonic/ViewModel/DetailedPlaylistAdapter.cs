using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playlists;
using WinSonic.Model.Api;

namespace WinSonic.ViewModel
{
    public class DetailedPlaylistAdapter
    {
        private readonly DetailedPlaylist originalPlaylist;
        public DetailedPlaylist Playlist { get; private set; }
        public ObservableCollection<Song> Songs { get; private set; }

        public DetailedPlaylistAdapter(DetailedPlaylist playlist)
        {
            Playlist = playlist;
            Songs = new ObservableCollection<Song>(playlist.Songs);
            originalPlaylist = new DetailedPlaylist(playlist);
        }

        public void SaveChanges()
        {
            Playlist.Songs.Clear();
            Playlist.Songs.AddRange(Songs);
        }

        public void UndoChanges()
        {
            Songs = new ObservableCollection<Song>(Playlist.Songs);
            Playlist.CopyValuesFrom(originalPlaylist);
        }
    }
}
