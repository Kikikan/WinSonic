using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages.Details;
using WinSonic.Pages.Favourites;
using WinSonic.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumDetailPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public InfoWithPicture? DetailedObject { get; set; }
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();

        public AlbumDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Store the item to be used in binding to UI
            if (e.Parameter is InfoWithPicture info)
            {
                DetailedObject = info;

                ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("OpenPictureControlItemAnimation");
                if (imageAnimation != null)
                {
                    // Connected animation + coordinated animation
                    imageAnimation.TryStart(detailedImage, new UIElement[] { coordinatedPanel });

                    ConnectedAnimation backImageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ArtistToAlbumAnimation");
                    if (backImageAnimation != null && DetailedObject?.BackIconUri != null)
                    {
                        backImageAnimation.TryStart(backImage);
                    }

                }
            }
        }

        // Create connected animation back to collection page.
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (e.SourcePageType == typeof(AlbumsPage) || e.SourcePageType == typeof(FavouriteAlbumPage) || e.SourcePageType == typeof(ArtistDetailPage))
            {
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ClosePictureControlItemAnimation", detailedImage);
                if (e.SourcePageType == typeof(ArtistDetailPage))
                {
                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("BackFromAlbumArtistAnimation", backImage);
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (DetailedObject != null)
            {
                var albumInfo = await SubsonicApiHelper.GetAlbumInfo(DetailedObject.ApiObject.Server, DetailedObject.ApiObject.Id);
                if (albumInfo != null)
                {
                    NoteTextBlock.Text = albumInfo.Notes;
                }
                var album = await SubsonicApiHelper.GetAlbum(DetailedObject.ApiObject.Server, DetailedObject.ApiObject.Id);
                if (album != null)
                {
                    foreach (var song in album.Song)
                    {
                        Songs.Add(new Song(song, DetailedObject.ApiObject.Server));
                    }
                }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var song in Songs)
            {
                PlayerPlaylist.Instance.AddSong(song);
            }
        }

        private void AlbumListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Song song)
            {
                PlayerPlaylist.Instance.AddSong(song);
            }
        }

        private async void FavouriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DetailedObject != null && DetailedObject.ApiObject is Album album)
            {
                bool success = await SubsonicApiHelper.Star(album.Server, !album.IsFavourite, SubsonicApiHelper.StarType.Album, album.Id);
                if (success)
                {
                    DetailedObject.IsFavourite = !DetailedObject.IsFavourite;
                    album.IsFavourite = !album.IsFavourite;
                    OnPropertyChanged(nameof(DetailedObject));
                }
            }
        }
    }
}
