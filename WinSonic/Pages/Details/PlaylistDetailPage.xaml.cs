using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using WinSonic.Controls;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages.Control;
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
        private bool IsOwner;

        public PlaylistDetailPage()
        {
            InitializeComponent();
            SongGridTable.Columns = [
                ("Track", new GridLength(80, GridUnitType.Pixel)),
                ("Title", new GridLength(4, GridUnitType.Star)),
                ("Artist", new GridLength(3, GridUnitType.Star)),
                ("Album", new GridLength(3, GridUnitType.Star)),
                ("Time", new GridLength(80, GridUnitType.Pixel))
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
                PlayerPlaylist.Instance.AddNextSong(Playlist.Songs[i]);
            }
        }

        private void SongGridTable_RowDoubleTapped(object sender, RowEvent e)
        {
            SongCommandBarFlyout.PlayNow(new CommandBarFlyout(), Playlist.Songs[e.Index], [.. Playlist.Songs], app.RoamingSettings.BehaviorSettings.PlaylistDoubleClickBehavior);
        }

        private CommandBarFlyout SongGridTable_RowRightTapped(object sender, RowEvent e)
        {
            return SongCommandBarFlyout.Create(Playlist.Playlist.Songs, Playlist.Songs[e.Index], SongGridTable, this, app.RoamingSettings.BehaviorSettings.PlaylistDoubleClickBehavior);
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

        private void SongGridTable_RowAdded(Microsoft.UI.Xaml.Shapes.Rectangle row, RowEvent e)
        {
            if (Playlist.Songs[e.Index].IsFavourite)
            {
                SongGridTable.RectangleColors[row] = true;
                SongGridTable.GetRectangle(e.Index).Fill = SongGridTable.Colors[true].Fill;
            }
        }
    }
}
