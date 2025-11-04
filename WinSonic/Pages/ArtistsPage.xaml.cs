using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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
        private readonly RoamingSettings roamingSettings = ((App)Application.Current).RoamingSettings;
        private readonly MainWindow? mainWindow = ((App)Application.Current).Window;
        private bool initialized = false;
        private readonly List<DetailedArtist> artists = [];

        public ArtistsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            if (Application.Current is App app)
            {
                app.RoamingSettings.ServerSettings.ServerChanged += ServerSettings_ServerChanged;
            }
        }

        private async void ServerSettings_ServerChanged(Model.Server server, Model.Settings.ServerSettingGroup.ServerOperation operation)
        {
            RefreshButton.IsEnabled = false;
            await Refresh();
            RefreshButton.IsEnabled = true;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialized)
            {
                await Refresh();
                initialized = true;
                RefreshButton.IsEnabled = true;
                if (mainWindow != null)
                {
                    mainWindow.SuggestionsChanged += MainWindow_SuggestionsChanged;
                }
            }
        }

        private async void MainWindow_SuggestionsChanged(object? sender, System.EventArgs e)
        {
            if (SearchResultsFilterCheckBox.IsChecked)
            {
                await Refresh();
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
            this.artists.Clear();
            ArtistControl.Items.Clear();
            List<InfoWithPicture> list = [];
            List<DetailedArtist> artists = [];
            if (!SearchResultsFilterCheckBox.IsChecked)
            {
                foreach (var server in roamingSettings.ServerSettings.ActiveServers.ToList())
                {
                    artists.AddRange(await SubsonicApiHelper.GetArtists(server));
                }
            }
            else if (mainWindow != null)
            {
                artists.AddRange([.. mainWindow.Suggestions.Where(x => x.Object is DetailedArtist).Select(x => (DetailedArtist) x.Object)]);
            }
            foreach (var artist in artists)
            {
                this.artists.Add(artist);
                ArtistControl.Items.Add(ToInfoWithPicture(artist));
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

            return SongCollectionCommandBarFlyout.Create(artists[index], artists[index], await SubsonicApiHelper.GetSongs(artists[index]), this, picture);
        }

        private async void SearchResultsFilterCheckBox_Click(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }
    }
}
