using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinSonic.Model.Api;
using WinSonic.Pages.Control;
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
    public FavouriteAlbumPage()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        foreach (var server in serverFile.Servers)
        {
            var rs = await SubsonicApiHelper.GetStarred(server);
            if (rs != null && rs.Middle != null)
            {
                foreach (var album in rs.Middle)
                {
                    PictureControl control = new PictureControl();
                    control.IconUri = album.CoverImageUrl;
                    control.Title = album.Title;
                    control.Subtitle = album.Artist;
                    PictureControl.Items.Add(control);
                }
            }
        }
    }
}
