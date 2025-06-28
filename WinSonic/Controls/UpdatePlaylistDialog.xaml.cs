using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinSonic.Model.Api;
using WinSonic.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Controls
{
    public sealed partial class UpdatePlaylistDialog : UserControl
    {
        public DetailedPlaylistAdapter Playlist { get; private set; }
        public UpdatePlaylistDialog(DetailedPlaylistAdapter playlist)
        {
            InitializeComponent();
            Playlist = playlist;
            SongListView.ItemsSource = Playlist.Songs;
        }

        private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            foreach (var song in SongListView.SelectedItems)
            {
                if (song is Song s)
                {
                    Playlist.Songs.Remove(s);
                }
            }
        }

        private void DeleteSong_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var song = button?.Tag as Song;
            if (song != null)
            {
                Playlist.Songs.Remove(song);
            }
        }

        private void SongListView_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Delete && SongListView.SelectedItem is Song song)
            {
                Playlist.Songs.Remove(song);
                e.Handled = true;
            }
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var deleteButton = grid?.FindName("DeleteButton") as Button;
            if (deleteButton != null)
            {
                deleteButton.Opacity = 1;
            }
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var deleteButton = grid?.FindName("DeleteButton") as Button;
            if (deleteButton != null)
            {
                deleteButton.Opacity = 0;
            }
        }
    }
}
