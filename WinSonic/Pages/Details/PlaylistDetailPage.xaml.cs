using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
using WinSonic.Pages.Dialog;
using WinSonic.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Details
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistDetailPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public InfoWithPicture? DetailedObject { get; set; }
        public DetailedPlaylistAdapter Playlist { get; private set; }
        private readonly App app = (App)Application.Current;
        private Song? RightClickedSong;
        private CommandBarFlyout? _songFlyout;
        private bool IsOwner;

        public PlaylistDetailPage()
        {
            InitializeComponent();
            SongGridTable.Columns = [
                new Tuple<string, GridLength>("Track", new GridLength(80, GridUnitType.Pixel)),
                new Tuple<string, GridLength>("Title", new GridLength(4, GridUnitType.Star)),
                new Tuple<string, GridLength>("Artist", new GridLength(3, GridUnitType.Star)),
                new Tuple<string, GridLength>("Album", new GridLength(3, GridUnitType.Star)),
                new Tuple<string, GridLength>("Time", new GridLength(80, GridUnitType.Pixel))
            ];
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is InfoWithPicture info)
            {
                DetailedObject = info;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (DetailedObject != null)
            {
                Playlist = new(await SubsonicApiHelper.GetPlaylist(DetailedObject.ApiObject.Server, DetailedObject.ApiObject.Id));
                OnPropertyChanged(nameof(Playlist));
                IsOwner = string.Equals(Playlist.Playlist.Owner, Playlist.Playlist.Server.Username, StringComparison.OrdinalIgnoreCase);
                OnPropertyChanged(nameof(IsOwner));
                int i = 1;
                foreach (var song in Playlist.Songs)
                {
                    TimeSpan duration = TimeSpan.FromSeconds(song.Duration);
                    Dictionary<string, string?> dic = new()
                    {
                        ["Track"] = string.Format("{0:D" + Playlist.Songs.Count.ToString().Length + "}", i),
                        ["Title"] = song.Title,
                        ["Artist"] = song.Artist,
                        ["Album"] = song.Album,
                        ["Time"] = string.Format("{0:D1}:{1:D2}", duration.Minutes, duration.Seconds),
                    };
                    SongGridTable.AddRow(dic);
                    i++;
                }
                SongGridTable.ShowContent();
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerPlaylist.Instance.ClearSongs();
            AddToQueueButton_Click(sender, e);
        }

        private void AddToQueueButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var song in Playlist.Songs)
            {
                PlayerPlaylist.Instance.AddSong(song);
            }
        }

        private void PlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = Playlist.Songs.Count - 1; i >= 0; i--)
            {
                PlayerPlaylist.Instance.AddSong(Playlist.Songs[i], (int)app.MediaPlaybackList.CurrentItemIndex + 1);
            }
        }

        private void SongGridTable_RowDoubleTapped(object sender, RowEvent e)
        {
            PlayerPlaylist.Instance.ClearSongs();
            PlayerPlaylist.Instance.AddSong(Playlist.Songs[e.Index]);
        }

        private CommandBarFlyout SongGridTable_RowRightTapped(object sender, RowEvent e)
        {
            RightClickedSong = Playlist.Songs[e.Index];
            OnPropertyChanged(nameof(RightClickedSong));
            _songFlyout = SongCommandBarFlyout.Create(RightClickedSong, SongPlayButton_Click, SongPlayNextButton_Click, SongAddToQueueButton_Click, SongFavouriteButton_Click, SongAddToPlaylistButton_Click);
            return _songFlyout;
        }

        private void SongPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (RightClickedSong != null)
            {
                PlayerPlaylist.Instance.ClearSongs();
                PlayerPlaylist.Instance.AddSong(RightClickedSong);
            }
            _songFlyout?.Hide();
        }

        private void SongPlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (RightClickedSong != null)
            {
                PlayerPlaylist.Instance.AddSong(RightClickedSong, (int)app.MediaPlaybackList.CurrentItemIndex + 1);
            }
            _songFlyout?.Hide();
        }

        private void SongAddToQueueButton_Click(object sender, RoutedEventArgs e)
        {
            if (RightClickedSong != null)
            {
                PlayerPlaylist.Instance.AddSong(RightClickedSong);
            }
            _songFlyout?.Hide();
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
            _songFlyout?.Hide();
        }

        private async void SongAddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            _songFlyout?.Hide();
            if (RightClickedSong != null)
            {
                var result = AddToPlaylistDialog.CreateDialog(this, RightClickedSong);
                AddToPlaylistDialog.ProcessDialog(await result.Item1.ShowAsync(), result.Item2);
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DetailedObject != null)
            {
                ContentDialog dialog = new()
                {
                    Title = "Editing playlist",
                    Content = new UpdatePlaylistDialog(Playlist),
                    PrimaryButtonText = "Update",
                    CloseButtonText = "Cancel",
                    XamlRoot = XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    DefaultButton = ContentDialogButton.Primary
                };
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    Playlist.SaveChanges();
                    await SubsonicApiHelper.UpdatePlaylist(Playlist.Playlist);
                    app?.Window?.NavFrame.GoBack();
                }
                else
                {
                    Playlist.UndoChanges();
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DetailedObject != null)
            {
                ContentDialog dialog = new()
                {
                    Title = "Are you sure?",
                    Content = "This action cannot be undone.",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    XamlRoot = XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    DefaultButton = ContentDialogButton.Close
                };
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await SubsonicApiHelper.DeletePlaylist(DetailedObject.ApiObject.Server, DetailedObject.ApiObject.Id);
                    app?.Window?.NavFrame.GoBack();
                }
            }
        }
    }
}
