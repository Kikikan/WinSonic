using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public ObservableCollection<Song> Songs { get; set; } = [];
        private readonly App app = (App)Application.Current;
        private Song? RightClickedSong;
        private CommandBarFlyout? _songFlyout;

        public PlaylistDetailPage()
        {
            InitializeComponent();
            SongGridTable.Columns = [
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
                var playlist = await SubsonicApiHelper.GetPlaylist(DetailedObject.ApiObject.Server, DetailedObject.ApiObject.Id);
                if (playlist != null)
                {
                    NoteTextBlock.Text = playlist.Comment;
                    foreach (var song in playlist.Entry)
                    {
                        Songs.Add(new Song(song, DetailedObject.ApiObject.Server));
                    }
                }

                foreach (var song in Songs)
                {
                    TimeSpan duration = TimeSpan.FromSeconds(song.Duration);
                    Dictionary<string, string?> dic = new()
                    {
                        ["Title"] = song.Title,
                        ["Artist"] = song.Artist,
                        ["Album"] = song.Album,
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

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DetailedObject != null)
            {
                await SubsonicApiHelper.DeletePlaylist(DetailedObject.ApiObject.Server, DetailedObject.ApiObject.Id);
                app?.Window?.NavFrame.GoBack();
            }

        }
    }
}
