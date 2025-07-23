using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using WinSonic.Controls;
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                await Refresh();
                initialized = true;
                RefreshButton.IsEnabled = true;
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            await Refresh();
            RefreshButton.IsEnabled = true;
        }

        private async Task<bool> Refresh()
        {
            artists.Clear();
            ArtistControl.Items.Clear();
            List<InfoWithPicture> list = [];
            foreach (var server in serverFile.ActiveServers.ToList())
            {
                var artists = await SubsonicApiHelper.GetArtists(server);
                foreach (var artist in artists)
                {
                    this.artists.Add(artist);
                    ArtistControl.Items.Add(ToInfoWithPicture(artist));
                }
            }
            ArtistControl.IsGrouped = true;
            return true;
        }

        private static InfoWithPicture ToInfoWithPicture(DetailedArtist artist)
        {
            return new InfoWithPicture(artist, artist.MediumImageUri, artist.Name, "", artist.IsFavourite, typeof(ArtistDetailPage), artist.Key);
        }

        private void FavouritesFilterCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (FavouritesFilterCheckBox.IsChecked)
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
            else
            {
                ArtistControl.Items.Clear();
                foreach (var artist in artists)
                {
                    ArtistControl.Items.Add(ToInfoWithPicture(artist));
                }
                ArtistControl.IsGrouped = true;
            }
        }

        private async Task<CommandBarFlyout> ArtistControl_RightTappedPicture(int index, InfoWithPicture picture)
        {

            return SongCollectionCommandBarFlyout.Create(artists[index], artists[index], await SubsonicApiHelper.GetSongs(artists[index]) ,this, picture);
        }
    }
}
