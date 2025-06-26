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
using WinSonic.Model;
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
        public Song Song { get; private set; }
        public AddToPlaylistDialog(Song song)
        {
            InitializeComponent();
            this.Song = song;
        }

        public static Tuple<ContentDialog, AddToPlaylistDialog> CreateDialog(Page page, Song song)
        {
            var content = new AddToPlaylistDialog(song);
            ContentDialog dialog = new()
            {
                XamlRoot = page.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Add song to playlist",
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
                    await SubsonicApiHelper.CreatePlaylist(dialog.Song.Server, dialog.NewNameTextBox.Text, dialog.Song.Id);
                }
                foreach (var obj in dialog.PlaylistList.SelectedItems)
                {
                    if (obj is Playlist playlist)
                    {
                        await SubsonicApiHelper.UpdatePlaylist(dialog.Song.Server, playlist.Id, dialog.Song.Id, true);
                    }
                }
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Playlist> ownerPlaylists = [];
            foreach (var playlist in await SubsonicApiHelper.GetPlaylists(Song.Server))
            {
                if (string.Equals(playlist.Owner, Song.Server.Username, StringComparison.OrdinalIgnoreCase))
                {
                    ownerPlaylists.Add(playlist);
                }
            }
            PlaylistList.ItemsSource = ownerPlaylists;
        }
    }
}
