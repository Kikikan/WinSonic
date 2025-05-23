using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using WinSonic.Model.Api;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Favourites;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FavouriteAlbumPage : Page
{
    private readonly ServerFile serverFile = ((App)Application.Current).ServerFile;
    private bool initialized = false;
    public FavouriteAlbumPage()
    {
        InitializeComponent();
        NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (!initialized)
        {
            foreach (var server in serverFile.Servers.Where(s => s.Enabled).ToList())
            {
                var rs = await SubsonicApiHelper.GetStarred(server);
                if (rs != null && rs.Middle != null)
                {
                    foreach (var album in rs.Middle)
                    {
                        PictureControl.Items.Add(new Model.InfoWithPicture(album, album.CoverImageUrl, album.Title, album.Artist, false, typeof(AlbumDetailPage), album.Title.Substring(0, 1)));
                    }
                }
            }
            initialized = true;
        }
    }
}
