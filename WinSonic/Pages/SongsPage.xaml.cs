using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SongsPage : Page, INotifyPropertyChanged
{
    private readonly RoamingSettings serverFile = ((App)Application.Current).RoamingSettings;
    private readonly List<Song> songList = [];
    private readonly List<Song> shownSongs = [];
    private bool initialized = false;
    private Song? RightClickedSong;
    private int? rightClickedIndex;
    private readonly App app = (App)Application.Current;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public SongsPage()
    {
        InitializeComponent();
        NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        GridTable.Columns = [
            new Tuple<string, GridLength>("Title", new GridLength(4, GridUnitType.Star)),
            new Tuple<string, GridLength>("Artist", new GridLength(2, GridUnitType.Star)),
            new Tuple<string, GridLength>("Album", new GridLength(3, GridUnitType.Star)),
            new Tuple<string, GridLength>("Time", new GridLength(80, GridUnitType.Pixel))
            ];
        GridTable.RowRightTapped += GridTable_RowRightTapped;
    }

    private CommandBarFlyout GridTable_RowRightTapped(object sender, Control.RowEvent e)
    {
        RightClickedSong = songList[e.Index];
        rightClickedIndex = e.Index;
        OnPropertyChanged(nameof(RightClickedSong));
        return SongFlyout;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (!initialized)
        {
            songList.Clear();
            foreach (var server in serverFile.Servers)
            {
                songList.AddRange(await SubsonicApiHelper.Search(server));
            }
            Refresh();
            initialized = true;
            RefreshButton.IsEnabled = true;
        }
    }

    private void GridTable_RowDoubleTapped(object sender, Control.RowEvent e)
    {
        PlayerPlaylist.Instance.ClearSongs();
        PlayerPlaylist.Instance.AddSong(songList[e.Index]);
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshButton.IsEnabled = false;
        songList.Clear();
        foreach (var server in serverFile.Servers)
        {
            songList.AddRange(await SubsonicApiHelper.Search(server));
        }
        Refresh();
        RefreshButton.IsEnabled = true;
    }

    private void Refresh()
    {
        shownSongs.Clear();
        GridTable.Clear();
        foreach (var song in songList)
        {
            if (!FavouritesFilterCheckBox.IsChecked || song.IsFavourite)
            {
                TimeSpan duration = TimeSpan.FromSeconds(song.Duration);
                Dictionary<string, string?> dic = new()
                {
                    ["Title"] = song.Title,
                    ["Artist"] = song.Artist,
                    ["Album"] = song.Album,
                    ["Time"] = string.Format("{0:D1}:{1:D2}", duration.Minutes, duration.Seconds),
                };
                GridTable.AddRow(dic);
                shownSongs.Add(song);
            }
        }
        GridTable.ShowContent();
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        if (RightClickedSong != null)
        {
            PlayerPlaylist.Instance.ClearSongs();
            PlayerPlaylist.Instance.AddSong(RightClickedSong);
        }
        SongFlyout.Hide();
    }

    private void PlayNextButton_Click(object sender, RoutedEventArgs e)
    {
        if (RightClickedSong != null)
        {
            PlayerPlaylist.Instance.AddSong(RightClickedSong, (int)app.MediaPlaybackList.CurrentItemIndex + 1);
        }
        SongFlyout.Hide();
    }

    private void AddToQueueButton_Click(object sender, RoutedEventArgs e)
    {
        if (RightClickedSong != null)
        {
            PlayerPlaylist.Instance.AddSong(RightClickedSong);
        }
        SongFlyout.Hide();
    }

    private async void FavouriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (RightClickedSong != null)
        {
            bool success = await SubsonicApiHelper.Star(RightClickedSong.Server, !RightClickedSong.IsFavourite, SubsonicApiHelper.StarType.Song, RightClickedSong.Id);
            if (success)
            {
                RightClickedSong.IsFavourite = !RightClickedSong.IsFavourite;
                if (rightClickedIndex != null)
                {
                    var rect = GridTable.GetRectangle((int)rightClickedIndex);
                    GridTable.RectangleColors[rect] = RightClickedSong.IsFavourite;
                    rect.Fill = GridTable.Colors[RightClickedSong.IsFavourite].Fill;
                }
                
                OnPropertyChanged(nameof(RightClickedSong));
            }
        }
        SongFlyout.Hide();
    }

    private void FavouritesFilterCheckBox_Click(object sender, RoutedEventArgs e)
    {
        Refresh();
    }

    private void GridTable_RowAdded(Microsoft.UI.Xaml.Shapes.Rectangle row, Control.RowEvent e)
    {
        if (shownSongs[e.Index].IsFavourite)
        {
            GridTable.RectangleColors[row] = true;
            GridTable.GetRectangle(e.Index).Fill = GridTable.Colors[true].Fill;
        }
    }
}
