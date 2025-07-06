using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using WinSonic.Model.Api;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Dialog
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddToPlaylistDialog : UserControl
    {
        public List<Song> Songs { get; private set; }
        private bool nameAlreadyExistsShown = false;
        public AddToPlaylistDialog(List<Song> songs)
        {
            InitializeComponent();
            Songs = songs;
        }

        public static (ContentDialog, AddToPlaylistDialog) CreateDialog(Page page, Song song)
        {
            return CreateDialog("song", page, [song]);
        }

        public static (ContentDialog, AddToPlaylistDialog) CreateDialog(Page page, DetailedArtist artist, List<Song> songs)
        {
            return CreateDialog($"{artist.Name}'s currently available songs", page, songs);
        }

        public static (ContentDialog, AddToPlaylistDialog) CreateDialog(Page page, Album album, List<Song> songs)
        {
            return CreateDialog($"currently available songs of {album.Title}", page, songs);
        }

        private static (ContentDialog, AddToPlaylistDialog) CreateDialog(string obj, Page page, List<Song> songs)
        {
            var content = new AddToPlaylistDialog(songs);
            ContentDialog dialog = new()
            {
                XamlRoot = page.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = $"Add {obj} to playlist",
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                Content = content
            };
            return new(dialog, content);
        }

        public static async void ProcessDialog(ContentDialogResult result, AddToPlaylistDialog dialog)
        {
            if (result == ContentDialogResult.Primary)
            {
                if (!string.IsNullOrWhiteSpace(dialog.NewNameTextBox.Text))
                {
                    await SubsonicApiHelper.CreatePlaylist(dialog.Songs[0].Server, dialog.NewNameTextBox.Text, [.. dialog.Songs.Select(s => s.Id)]);
                }
                foreach (var obj in dialog.PlaylistList.SelectedItems)
                {
                    if (obj is Playlist playlist)
                    {
                        await SubsonicApiHelper.UpdatePlaylist(dialog.Songs[0].Server, playlist.Id, null, null, null, [], [.. dialog.Songs.Select(s => s.Id)]);
                    }
                }
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Playlist> ownerPlaylists = [];
            foreach (var playlist in await SubsonicApiHelper.GetPlaylists(Songs[0].Server))
            {
                if (string.Equals(playlist.Owner, Songs[0].Server.Username, StringComparison.OrdinalIgnoreCase))
                {
                    ownerPlaylists.Add(playlist);
                }
            }
            PlaylistList.ItemsSource = ownerPlaylists;
        }

        private void NewNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool exists = ((List<Playlist>)PlaylistList.ItemsSource).Exists(p => string.Equals(p.Name, NewNameTextBox.Text));
            if (exists && !nameAlreadyExistsShown)
            {
                NameExistsInfoBar.IsOpen = true;
                nameAlreadyExistsShown = true;
            }
            else if (!exists && NameExistsInfoBar.IsOpen)
            {
                NameExistsInfoBar.IsOpen = false;
            }
        }
    }
}
