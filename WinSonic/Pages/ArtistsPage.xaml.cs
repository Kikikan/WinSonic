using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using WinSonic.Model.Api;
using WinSonic.Pages.Details;
using WinSonic.Persistence;
using WinSonic.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ArtistsPage : Page
    {
        private readonly RoamingSettings serverFile = ((App)Application.Current).RoamingSettings;
        private bool initialized = false;

        public ArtistsPage()
        {
            InitializeComponent();
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                List<InfoWithPicture> list = [];
                foreach (var server in serverFile.Servers.Where(s => s.Enabled).ToList())
                {
                    var artists = await SubsonicApiHelper.GetArtists(server);
                    foreach (var artist in artists)
                    {
                        ArtistControl.Items.Add(new InfoWithPicture(artist, artist.MediumImageUri, artist.Name, "", artist.IsFavourite, typeof(ArtistDetailPage), artist.Key));
                    }
                }
                initialized = true;
                ArtistControl.IsGrouped = true;
            }
        }
    }
}
