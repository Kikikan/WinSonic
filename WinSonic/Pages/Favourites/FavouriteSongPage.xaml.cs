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
public sealed partial class FavouriteSongPage : Page
{
    private readonly ServerFile serverFile = ((App)Application.Current).ServerFile;
    private bool initialized = false;
    public FavouriteSongPage()
    {
        InitializeComponent();
        NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (!initialized)
        {
            foreach (var server in serverFile.Servers)
            {
                var rs = await SubsonicApiHelper.GetStarred(server);
                if (rs != null && rs.Right != null)
                {
                    foreach (var song in rs.Right)
                    {
                        PictureControl control = new PictureControl();
                        control.IconUri = song.CoverImageUri;
                        control.Title = song.Title;
                        control.Subtitle = song.Artist;
                        control.DetailsType = typeof(SongDetailPage);
                        PictureControl.Items.Add(control);
                    }
                }
            }
            initialized = true;
        }
    }
}
