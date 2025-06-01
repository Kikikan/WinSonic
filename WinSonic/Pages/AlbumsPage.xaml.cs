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
        private readonly List<Tuple<string, SubsonicApiHelper.AlbumListType>> SortingMethods = [
            Tuple.Create("Newest", SubsonicApiHelper.AlbumListType.newest),
            Tuple.Create("By Name", SubsonicApiHelper.AlbumListType.alphabeticalByName),
            Tuple.Create("By Artist", SubsonicApiHelper.AlbumListType.alphabeticalByArtist),
            Tuple.Create("Favourites", SubsonicApiHelper.AlbumListType.starred),
        ];
        private bool initialized = false;

        public AlbumsPage()
        {
            InitializeComponent();
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private async Task<bool> Update()
        {
            bool result = false;
            foreach (var server in serverFile.Servers.Where(s => s.Enabled).ToList())
            {
                List<Album> albums = await SubsonicApiHelper.GetAlbumList(server, ((Tuple<string, SubsonicApiHelper.AlbumListType>)OrderByComboBox.SelectedItem).Item2, 10, AlbumControl.Items.Count);
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

        private void OrderByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!initialized)
            {
                return;
            }
            if (OrderByComboBox.SelectedItem is Tuple<string, SubsonicApiHelper.AlbumListType> selected)
            {
                serverFile.AlbumSettings.OrderBy = selected.Item2;
                serverFile.SaveSetting(serverFile.AlbumSettings);
                Refresh();
            }
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
                OrderByComboBox.SelectedItem = SortingMethods.Where(t => t.Item2 == serverFile.AlbumSettings.OrderBy).First();
                AlbumControl.UpdateAction = Update;
                initialized = true;
            }
        }
    }
}
