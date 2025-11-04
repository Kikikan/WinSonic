using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinSonic.Controls;
using WinSonic.Model.Api;
using WinSonic.Persistence;
using WinSonic.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumsPage : Page
    {
        private readonly RoamingSettings roamingSettings = ((App)Application.Current).RoamingSettings;
        private readonly MainWindow? mainWindow = ((App)Application.Current).Window;
        private SubsonicApiHelper.AlbumListType OrderBy;
        private bool initialized = false;
        private readonly List<Album> albums = [];

        public AlbumsPage()
        {
            InitializeComponent();
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            OrderBy = roamingSettings.AlbumSettings.OrderBy;
            if (Application.Current is App app)
            {
                app.RoamingSettings.ServerSettings.ServerChanged += ServerSettings_ServerChanged;
            }
        }

        private async Task<bool> Update()
        {
            bool added = false;
            bool result = false;
            List<Album> albums = [];
            if (!SearchResultsFilterCheckBox.IsChecked)
            {
                foreach (var server in roamingSettings.ServerSettings.ActiveServers)
                {
                    albums.AddRange(await SubsonicApiHelper.GetAlbumList(server, OrderBy, 12, this.albums.Count));
                }
            }
            else
            {
                var suggestions = mainWindow?.Suggestions;
                if (suggestions != null)
                {
                    albums.AddRange([.. suggestions.Where(s => s.Object is Album).Select(s => (Album)s.Object)]);
                }
            }
            foreach (var album in albums.ToList())
            {
                if (this.albums.Contains(album))
                {
                    albums.Remove(album);
                }
            }
            this.albums.AddRange(albums);
            if (albums != null && albums.Count > 0)
            {
                foreach (var album in albums)
                {
                    if (!FavouritesFilterCheckBox.IsChecked || album.IsFavourite)
                    {
                        AlbumControl.Items.Add(new InfoWithPicture(album, album.CoverImageUrl, album.Title, album.Artist, album.IsFavourite, typeof(AlbumDetailPage), album.Title[..1]));
                        added = true;
                    }
                }
                result = true;
            }
            if (result && !added)
            {
                await Update();
            }
            return result;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        private async void ServerSettings_ServerChanged(Model.Server server, Model.Settings.ServerSettingGroup.ServerOperation operation)
        {
            await Refresh();
        }

        private async Task Refresh()
        {
            albums.Clear();
            AlbumControl.Items.Clear();
            // Wait until UI updates
            await Task.Delay(100);
            AlbumControl.UpdateAction = Update;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                switch (roamingSettings.AlbumSettings.OrderBy)
                {
                    case SubsonicApiHelper.AlbumListType.newest:
                        NewestRadioItem.IsChecked = true;
                        break;
                    case SubsonicApiHelper.AlbumListType.alphabeticalByName:
                        NameRadioItem.IsChecked = true;
                        break;
                    case SubsonicApiHelper.AlbumListType.alphabeticalByArtist:
                        ArtistRadioItem.IsChecked = true;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                AlbumControl.UpdateAction = Update;
                if (mainWindow != null)
                {
                    mainWindow.SuggestionsChanged += MainWindow_SuggestionsChanged;
                }
                initialized = true;
            }
        }

        private async void MainWindow_SuggestionsChanged(object? sender, EventArgs e)
        {
            if (SearchResultsFilterCheckBox.IsChecked)
            {
                await Refresh();
            }
        }

        private async void FavouritesFilterCheckBox_Click(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        private async void RadioItem_Click(object sender, RoutedEventArgs e)
        {
            if (NewestRadioItem.IsChecked)
            {
                OrderBy = SubsonicApiHelper.AlbumListType.newest;
            }
            else if (NameRadioItem.IsChecked)
            {
                OrderBy = SubsonicApiHelper.AlbumListType.alphabeticalByName;
            }
            else
            {
                OrderBy = SubsonicApiHelper.AlbumListType.alphabeticalByArtist;
            }
            roamingSettings.AlbumSettings.OrderBy = OrderBy;
            roamingSettings.SaveSetting(roamingSettings.AlbumSettings);
            await Refresh();
        }

        private async Task<CommandBarFlyout> AlbumControl_RightTappedPicture(int index, InfoWithPicture picture)
        {
            var album = albums[index];
            return SongCollectionCommandBarFlyout.Create(album, album, await SubsonicApiHelper.GetSongs(album), this, picture);
        }
    }
}
