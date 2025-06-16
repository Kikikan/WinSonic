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
        private readonly RoamingSettings serverFile = ((App)Application.Current).RoamingSettings;
        private SubsonicApiHelper.AlbumListType OrderBy;
        private bool initialized = false;

        public AlbumsPage()
        {
            InitializeComponent();
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            OrderBy = serverFile.AlbumSettings.OrderBy;
        }

        private async Task<bool> Update()
        {
            bool result = false;
            foreach (var server in serverFile.Servers.Where(s => s.Enabled).ToList())
            {
                List<Album> albums = await SubsonicApiHelper.GetAlbumList(server, OrderBy, 10, AlbumControl.Items.Count);
                if (albums != null && albums.Count > 0)
                {
                    foreach (var album in albums)
                    {
                        AlbumControl.Items.Add(new InfoWithPicture(album, album.CoverImageUrl, album.Title, album.Artist, album.IsFavourite, typeof(AlbumDetailPage), album.Title.Substring(0, 1)));
                    }
                    result = true;
                }
            }
            return result;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private async void Refresh()
        {
            AlbumControl.Items.Clear();
            // Wait until UI updates
            await Task.Delay(100);
            AlbumControl.UpdateAction = Update;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                switch (serverFile.AlbumSettings.OrderBy)
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
            serverFile.AlbumSettings.OrderBy = OrderBy;
            serverFile.SaveSetting(serverFile.AlbumSettings);
            Refresh();
        }
    }
}
