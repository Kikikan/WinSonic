using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinSonic.Model;
using WinSonic.Model.Api;
using WinSonic.Pages.Base;
using WinSonic.Pages.Control;
using WinSonic.Pages.Details;
using WinSonic.Persistence;
using WinSonic.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistPage : ApiPage
    {
        private readonly RoamingSettings roamingSettings = ((App)Application.Current).RoamingSettings;
        private bool initialized = false;
        private readonly List<Playlist> playlists = [];
        private readonly Dictionary<Playlist, Server> playlistServerMap = [];
        public PlaylistPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            PlaylistGridTable.Columns = [
                ("Name", new GridLength(4, GridUnitType.Star)),
                ("Description", new GridLength(3, GridUnitType.Star)),
                ("Owner", new GridLength(2, GridUnitType.Star)),
                ("Tracks", new GridLength(80, GridUnitType.Pixel))
            ];
            roamingSettings.ServerSettings.ServerChanged += ServerSettings_ServerChanged;
        }

        private async void ServerSettings_ServerChanged(Server server, Model.Settings.ServerSettingGroup.ServerOperation operation)
        {
            await Refresh();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                await InitializeCollections();
                RefreshGridTable();
            }
        }

        private void PlaylistGridTable_RowTapped(object sender, RowEvent e)
        {
            if (Application.Current is App app)
            {
                var selected = playlists[e.Index];
                InfoWithPicture playlistInfo = new(new ApiObject(selected.Id, playlistServerMap[selected]), null, selected.Name, selected.Owner, false, typeof(AlbumDetailPage), selected.Name[..1]);
                app.Window?.NavFrame.Navigate(typeof(PlaylistDetailPage), playlistInfo, new DrillInNavigationTransitionInfo());
            }
        }

        private void PlaylistGridTable_RowAdded(Microsoft.UI.Xaml.Shapes.Rectangle row, RowEvent e)
        {
            if (string.Equals(playlists[e.Index].Owner, playlistServerMap[playlists[e.Index]].Username, StringComparison.OrdinalIgnoreCase))
            {
                PlaylistGridTable.RectangleColors[row] = true;
                PlaylistGridTable.GetRectangle(e.Index).Fill = PlaylistGridTable.Colors[true].Fill;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            if (!initialized)
            {
                await InitializeCollections();
                RefreshGridTable();
                initialized = true;
            }
            RefreshButton.IsEnabled = true;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }
        
        private async Task Refresh()
        {
            RefreshButton.IsEnabled = false;
            await InitializeCollections();
            RefreshGridTable();
            RefreshButton.IsEnabled = true;
        }

        private async Task InitializeCollections()
        {
            playlistServerMap.Clear();
            playlists.Clear();
            foreach (var server in roamingSettings.ServerSettings.ActiveServers)
            {
                var playlists = await TryApiCall(() => SubsonicApiHelper.GetPlaylists(server));
                if (playlists != null)
                {
                    foreach (var playlist in playlists)
                    {
                        this.playlists.Add(playlist);
                        playlistServerMap.Add(playlist, server);
                    }
                }
                
            }
        }

        private void RefreshGridTable()
        {
            PlaylistGridTable.Clear();
            foreach (var playlist in playlists)
            {
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
