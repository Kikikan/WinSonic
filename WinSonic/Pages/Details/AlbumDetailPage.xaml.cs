using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages.Control;
using WinSonic.Pages.Details;
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
        public ObservableCollection<Song> Songs { get; set; } = [];
        private readonly App app = (App)Application.Current;
        private Song? RightClickedSong;

        public AlbumDetailPage()
        {
            InitializeComponent();
            SongGridTable.Columns = [
                new Tuple<string, GridLength>("Track", new GridLength(80, GridUnitType.Pixel)),
                new Tuple<string, GridLength>("Title", new GridLength(4, GridUnitType.Star)),
                new Tuple<string, GridLength>("Artist", new GridLength(3, GridUnitType.Star)),
                new Tuple<string, GridLength>("Time", new GridLength(80, GridUnitType.Pixel))
            ];
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
                    imageAnimation.TryStart(detailedImage, [coordinatedPanel]);

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

            if (e.NavigationMode == NavigationMode.Back)
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
                foreach (var song in Songs)
                {
                    TimeSpan duration = TimeSpan.FromSeconds(song.Duration);
                    Dictionary<string, string?> dic = new()
                    {
                        ["Track"] = string.Format("{0:D1}.{1:D2}", song.DiskNumber, song.Track),
                        ["Title"] = song.Title,
                        ["Artist"] = song.Artist,
                        ["Time"] = string.Format("{0:D1}:{1:D2}", duration.Minutes, duration.Seconds),
                    };
                    SongGridTable.AddRow(dic);
                }
                SongGridTable.ShowContent();
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerPlaylist.Instance.ClearSongs();
            AddToQueueButton_Click(sender, e);
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

        private void AddToQueueButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var song in Songs)
            {
                PlayerPlaylist.Instance.AddSong(song);
            }
        }

        private void PlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = Songs.Count - 1; i >= 0; i--)
            {
                PlayerPlaylist.Instance.AddSong(Songs[i], (int)app.MediaPlaybackList.CurrentItemIndex + 1);
            }
        }

        private void SongGridTable_RowDoubleTapped(object sender, RowEvent e)
        {
            PlayerPlaylist.Instance.ClearSongs();
            PlayerPlaylist.Instance.AddSong(Songs[e.Index]);
        }

        private CommandBarFlyout SongGridTable_RowRightTapped(object sender, RowEvent e)
        {
            RightClickedSong = Songs[e.Index];
            OnPropertyChanged(nameof(RightClickedSong));
            return SongFlyout;
        }

        private void SongPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (RightClickedSong != null)
            {
                PlayerPlaylist.Instance.ClearSongs();
                PlayerPlaylist.Instance.AddSong(RightClickedSong);
            }
            SongFlyout.Hide();
        }

        private void SongPlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (RightClickedSong != null)
            {
                PlayerPlaylist.Instance.AddSong(RightClickedSong, (int)app.MediaPlaybackList.CurrentItemIndex + 1);
            }
            SongFlyout.Hide();
        }

        private void SongAddToQueueButton_Click(object sender, RoutedEventArgs e)
        {
            if (RightClickedSong != null)
            {
                PlayerPlaylist.Instance.AddSong(RightClickedSong);
            }
            SongFlyout.Hide();
        }

        private async void SongFavouriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (RightClickedSong != null)
            {
                bool success = await SubsonicApiHelper.Star(RightClickedSong.Server, !RightClickedSong.IsFavourite, SubsonicApiHelper.StarType.Song, RightClickedSong.Id);
                if (success)
                {
                    RightClickedSong.IsFavourite = !RightClickedSong.IsFavourite;
                    OnPropertyChanged(nameof(RightClickedSong));
                }
            }
            SongFlyout.Hide();
        }
    }
}
