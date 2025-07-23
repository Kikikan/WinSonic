using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WinSonic.Controls;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages.Control;
using WinSonic.Pages.Details;
using WinSonic.Pages.Dialog;
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

        public AlbumDetailPage()
        {
            InitializeComponent();
            SongGridTable.Columns = [
                ("Track", new GridLength(80, GridUnitType.Pixel)),
                ("Title", new GridLength(4, GridUnitType.Star)),
                ("Artist", new GridLength(3, GridUnitType.Star)),
                ("Time", new GridLength(80, GridUnitType.Pixel))
            ];
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Store the item to be used in binding to UI
            if (e.Parameter is InfoWithPicture info)
            {
                DetailedObject = info;
                CommandBar.ShownObj = DetailedObject;
                if (DetailedObject?.ApiObject is Album album)
                {
                    CommandBar.ApiObj = DetailedObject.ApiObject;
                    CommandBar.FavObj = album;
                    CommandBar.IsFavourite = album.IsFavourite;
                }

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
                        var s = new Song(song, DetailedObject.ApiObject.Server);
                        Songs.Add(s);
                        CommandBar.Songs.Add(s);
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

        private void SongGridTable_RowDoubleTapped(object sender, RowEvent e)
        {
            SongCommandBarFlyout.PlayNow(new CommandBarFlyout(), Songs[e.Index], [.. Songs], app.RoamingSettings.BehaviorSettings.AlbumDoubleClickBehavior);
        }

        private CommandBarFlyout SongGridTable_RowRightTapped(object sender, RowEvent e)
        {
            return SongCommandBarFlyout.Create([.. Songs], Songs[e.Index], SongGridTable, this, app.RoamingSettings.BehaviorSettings.AlbumDoubleClickBehavior);
        }

        private void SongGridTable_RowAdded(Microsoft.UI.Xaml.Shapes.Rectangle row, RowEvent e)
        {
            if (Songs[e.Index].IsFavourite)
            {
                SongGridTable.RectangleColors[row] = true;
                SongGridTable.GetRectangle(e.Index).Fill = SongGridTable.Colors[true].Fill;
            }
        }
    }
}
