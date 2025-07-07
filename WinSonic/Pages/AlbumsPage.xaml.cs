using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private SubsonicApiHelper.AlbumListType OrderBy;
        private bool initialized = false;
        private readonly List<Album> albums = [];

        public AlbumsPage()
        {
            InitializeComponent();
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            OrderBy = roamingSettings.AlbumSettings.OrderBy;
        }

        private async Task<bool> Update()
        {
            bool added = false;
            bool result = false;
            foreach (var server in roamingSettings.ServerSettings.ActiveServers.ToList())
            {
                List<Album> albums = await SubsonicApiHelper.GetAlbumList(server, OrderBy, 12, this.albums.Count);
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
            }
            if (result && !added)
            {
                await Update();
            }
            return result;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private async void Refresh()
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
                initialized = true;
            }
        }
        private void FavouritesFilterCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void RadioItem_Click(object sender, RoutedEventArgs e)
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
            Refresh();
        }
    }
}
