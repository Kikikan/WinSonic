using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using WinSonic.Model;
using WinSonic.Model.Api;
using WinSonic.Pages.Control;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistPage : Page
    {
        private readonly RoamingSettings roamingSettings = ((App)Application.Current).RoamingSettings;
        private bool initialized = false;
        private readonly List<Playlist> playlists = [];
        private readonly Dictionary<Playlist, Server> playlistServerMap = [];
        public PlaylistPage()
        {
            InitializeComponent();
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            PlaylistGridTable.Columns = [
                new Tuple<string, GridLength>("Name", new GridLength(4, GridUnitType.Star)),
                new Tuple<string, GridLength>("Description", new GridLength(3, GridUnitType.Star)),
                new Tuple<string, GridLength>("Owner", new GridLength(2, GridUnitType.Star)),
                new Tuple<string, GridLength>("Tracks", new GridLength(80, GridUnitType.Pixel))
            ];
        }

        private void PlaylistGridTable_RowDoubleTapped(object sender, RowEvent e)
        {

        }

        private void PlaylistGridTable_RowAdded(Microsoft.UI.Xaml.Shapes.Rectangle row, RowEvent e)
        {
            if (playlists[e.Index].Owner.ToLower() == playlistServerMap[playlists[e.Index]].Username.ToLower())
            {
                PlaylistGridTable.RectangleColors[row] = true;
                PlaylistGridTable.GetRectangle(e.Index).Fill = PlaylistGridTable.Colors[true].Fill;
            }
        }

        private CommandBarFlyout? PlaylistGridTable_RowRightTapped(object sender, RowEvent e)
        {
            return null;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                playlistServerMap.Clear();
                playlists.Clear();
                foreach (var server in roamingSettings.ActiveServers)
                {
                    foreach (var playlist in await SubsonicApiHelper.GetPlaylists(server))
                    {
                        playlists.Add(playlist);
                        playlistServerMap.Add(playlist, server);
                    }
                }
                Refresh();
                initialized = true;
                RefreshButton.IsEnabled = true;
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            playlists.Clear();
            foreach (var server in roamingSettings.ActiveServers)
            {
                playlists.AddRange(await SubsonicApiHelper.GetPlaylists(server));
            }
            Refresh();
            RefreshButton.IsEnabled = true;
        }

        private void Refresh()
        {
            PlaylistGridTable.Clear();
            foreach (var playlist in playlists)
            {
                TimeSpan duration = TimeSpan.FromSeconds(playlist.Duration);
                Dictionary<string, string?> dic = new()
                {
                    ["Name"] = playlist.Name,
                    ["Description"] = playlist.Comment,
                    ["Owner"] = playlist.Owner,
                    ["Tracks"] = playlist.SongCount.ToString(),
                };
                PlaylistGridTable.AddRow(dic);
            }
            PlaylistGridTable.ShowContent();
        }
    }
}
