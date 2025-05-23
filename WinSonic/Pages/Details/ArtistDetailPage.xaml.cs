using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using WinSonic.Model;
using WinSonic.Model.Api;
using WinSonic.Pages.Favourites;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Details;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ArtistDetailPage : Page
{

    public InfoWithPicture DetailedObject { get; set; }
    private bool _initialized = false;
    public ArtistDetailPage()
    {
        InitializeComponent();
        NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.NavigationMode == NavigationMode.Back)
        {
            ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackImageAnimation");
            if (imageAnimation != null)
            {
                // Connected animation + coordinated animation
                imageAnimation.TryStart(detailedImage, new UIElement[] {coordinatedPanel});
            }
        }
        else
        {
            // Store the item to be used in binding to UI
            DetailedObject = e.Parameter as InfoWithPicture;

            ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
            if (imageAnimation != null)
            {
                // Connected animation + coordinated animation
                imageAnimation.TryStart(detailedImage, new UIElement[] { coordinatedPanel });
            }
        }
    }

    // Create connected animation back to collection page.
    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        
        
        if (e.SourcePageType == typeof(ArtistsPage))
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("BackConnectedAnimation", detailedImage);
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }
        else if (e.SourcePageType == typeof(FavouriteArtistPage))
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("BackImageAnimation", detailedImage);
        }
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_initialized)
        {
            var artistInfo = await SubsonicApiHelper.GetArtist(DetailedObject.ApiObject.Server, DetailedObject.ApiObject.Id);
            foreach (var album in artistInfo.Album)
            {
                Album albumObj = new Album(album, DetailedObject.ApiObject.Server);
                InfoWithPicture a = new InfoWithPicture(albumObj, albumObj.CoverImageUrl, albumObj.Title, albumObj.Artist, albumObj.IsFavourite, typeof(AlbumDetailPage), "", ((DetailedArtist)DetailedObject.ApiObject).SmallImageUri);
                AlbumControl.Items.Add(a);
            }

            var artistInfo2 = await SubsonicApiHelper.GetArtistInfo(DetailedObject.ApiObject.Server, DetailedObject.ApiObject.Id);
            if (artistInfo2 != null)
            {
                NoteTextBlock.Text = artistInfo2.Biography;
            }
            _initialized = true;
        }
    }
}
