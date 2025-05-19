using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public sealed partial class AlbumsPage : Page
    {

        private readonly ServerFile serverFile = ((App)Application.Current).ServerFile;

        public AlbumsPage()
        {
            InitializeComponent();
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            AlbumControl.UpdateAction = (count) => Update(count);
        }

        private async Task<bool> Update(int offset)
        {
            bool result = false;
            foreach (var server in serverFile.Servers)
            {
                List<Album> albums = await SubsonicApiHelper.GetAlbumList(server, SubsonicApiHelper.AlbumListType.newest, 20, offset);
                if (albums != null && albums.Count > 0)
                {
                    foreach (var album in albums)
                    {
                        PictureControl control = new PictureControl();
                        control.ApiObject = album;
                        control.IconUri = album.CoverImageUrl;
                        control.Title = album.Title;
                        control.Subtitle = album.Artist;
                        control.IsFavourite = album.IsFavourite;
                        control.DetailsType = typeof(AlbumDetailPage);
                        AlbumControl.Items.Add(control);
                    }
                    result = true;
                }
            }
            return result;
        }
    }
}
