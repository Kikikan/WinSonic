using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private readonly List<DetailedArtist> artists = [];

        public ArtistsPage()
        {
            InitializeComponent();
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                Refresh();
                initialized = true;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private async void Refresh()
        {
            artists.Clear();
            ArtistControl.Items.Clear();
            List<InfoWithPicture> list = [];
            foreach (var server in serverFile.Servers.Where(s => s.Enabled).ToList())
            {
                var artists = await SubsonicApiHelper.GetArtists(server);
                foreach (var artist in artists)
                {
                    this.artists.Add(artist);
                    ArtistControl.Items.Add(ToInfoWithPicture(artist));
                }
            }
            ArtistControl.IsGrouped = true;
        }

        private void FavouritesFilterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var control in ArtistControl.Items.ToImmutableList())
            {
                if (!control.IsFavourite)
                {
                    ArtistControl.Items.Remove(control);
                }
            }
            ArtistControl.IsGrouped = true;
        }

        private void FavouritesFilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ArtistControl.Items.Clear();
            foreach (var artist in artists)
            {
                ArtistControl.Items.Add(ToInfoWithPicture(artist));
            }
            ArtistControl.IsGrouped = true;
        }

        private static InfoWithPicture ToInfoWithPicture(DetailedArtist artist)
        {
            return new InfoWithPicture(artist, artist.MediumImageUri, artist.Name, "", artist.IsFavourite, typeof(ArtistDetailPage), artist.Key);
        }
    }
}
