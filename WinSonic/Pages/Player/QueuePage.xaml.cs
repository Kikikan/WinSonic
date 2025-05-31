using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using Windows.Media.Playback;
using WinSonic.Model.Api;
using WinSonic.Model.Player;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Player
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QueuePage : Page
    {
        public ObservableCollection<Song> Songs { get; set; } = [];
        private readonly MediaPlaybackList Playlist;
        private Song? SelectedSong { get; set; }

        public QueuePage()
        {
            InitializeComponent();
            if (Application.Current is App app)
            {
                Playlist = app.MediaPlaybackList;
            }
            else
            {
                throw new System.Exception("Application is not App.");
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ListSongs();
        }

        private void PlaylistView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue || args.ItemContainer == null)
            {
                return;
            }
            // TODO: Implement reordering again!
        }

        private void SongContainer_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (sender is ItemContainer container && container.DataContext is Song song)
            {
                Playlist.MoveTo((uint)Songs.IndexOf(song));
            }
        }

        private void SongContainer_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is Song song)
            {
                SelectedSong = song;
            }
        }

        private void SongPlayItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSong != null)
            {
                Play(Songs.IndexOf(SelectedSong));
            }
        }

        private void SongRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSong != null)
            {
                Remove(Songs.IndexOf(SelectedSong));
            }
        }

        private void ListSongs()
        {
            Songs.Clear();
            foreach (var song in PlayerPlaylist.Instance.Songs)
            {
                Songs.Add(song);
            }
        }

        private void Play(int index)
        {
            Playlist.MoveTo((uint)index);
        }

        private void Remove(int index)
        {
            PlayerPlaylist.Instance.RemoveSong(index);
            ListSongs();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerPlaylist.Instance.ClearSongs();
            Songs.Clear();
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException(); // TODO
        }
    }
}
