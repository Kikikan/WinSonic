using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;
using WinSonic.Model.Api;
using WinSonic.Pages.Details;
using WinSonic.Persistence;
using WinSonic.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Favourites;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FavouriteArtistPage : Page
{
    private readonly ServerFile serverFile = ((App)Application.Current).ServerFile;
    private bool initialized = false;
    public FavouriteArtistPage()
    {
        InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (!initialized)
        {
            foreach (var server in serverFile.Servers.Where(s => s.Enabled).ToList())
            {
                var rs = await SubsonicApiHelper.GetStarred(server);
                if (rs != null && rs.Left != null)
                {
                    foreach (var artist in rs.Left)
                    {
                        var artistRs = await SubsonicApiHelper.GetArtistInfo(server, artist.Id);
                        DetailedArtist detailedArtist = new(server, artist.Name[..1], artist.Id, artist.Name, artistRs.Biography, artist.StarredSpecified, artistRs.SmallImageUrl, artistRs.MediumImageUrl, artistRs.LargeImageUrl);
                        PictureControl.Items.Add(new InfoWithPicture(detailedArtist, detailedArtist.MediumImageUri, detailedArtist.Name, "", artist.StarredSpecified, typeof(ArtistDetailPage), detailedArtist.Key));
                    }
                }
            }
            initialized = true;
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        if (e.SourcePageType != typeof(ArtistDetailPage))
        {
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }
    }
}
