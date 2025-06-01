using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

        public AlbumsPage()
        {
            InitializeComponent();
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            AlbumControl.UpdateAction = Update;
        }

        private async Task<bool> Update()
        {
            bool result = false;
            foreach (var server in serverFile.Servers.Where(s => s.Enabled).ToList())
            {
                List<Album> albums = await SubsonicApiHelper.GetAlbumList(server, SubsonicApiHelper.AlbumListType.newest, 10, AlbumControl.Items.Count);
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

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            AlbumControl.Items.Clear();
            // Wait until UI updates
            await Task.Delay(100);
            AlbumControl.UpdateAction = Update;
        }
    }
}
