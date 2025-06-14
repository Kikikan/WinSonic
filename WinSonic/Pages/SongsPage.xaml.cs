using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SongsPage : Page
{
    private readonly RoamingSettings serverFile = ((App)Application.Current).RoamingSettings;
    private readonly List<Song> songList = [];
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
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Refresh();
    }

    private void GridTable_RowDoubleTapped(object sender, Control.RowEvent e)
    {
        PlayerPlaylist.Instance.ClearSongs();
        PlayerPlaylist.Instance.AddSong(songList[e.Index]);
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        Refresh();
    }

    private async void Refresh()
    {
        songList.Clear();
        GridTable.Clear();
        foreach (var server in serverFile.Servers)
        {
            songList.AddRange(await SubsonicApiHelper.Search(server));
        }
        foreach (var song in songList)
        {
            TimeSpan duration = TimeSpan.FromSeconds(song.Duration);

            Dictionary<string, string?> dic = new()
            {
                ["Title"] = song.Title,
                ["Artist"] = song.Artist,
                ["Album"] = song.Album,
                ["Time"] = string.Format("{0:D1}:{1:D2}",
                            duration.Minutes,
                            duration.Seconds),
            };
            GridTable.AddRow(dic);
        }
        GridTable.ShowContent();
    }
}
