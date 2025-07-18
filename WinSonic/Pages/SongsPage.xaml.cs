using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using WinSonic.Controls;
using WinSonic.Model.Api;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SongsPage : Page
{
    private readonly RoamingSettings roamingSettings = ((App)Application.Current).RoamingSettings;
    private readonly List<Song> songList = [];
    private readonly List<Song> shownSongs = [];
    private bool initialized = false;

    public SongsPage()
    {
        InitializeComponent();
        NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        GridTable.Columns = [
            ("Title", new GridLength(4, GridUnitType.Star)),
            ("Artist", new GridLength(2, GridUnitType.Star)),
            ("Album", new GridLength(3, GridUnitType.Star)),
            ("Time", new GridLength(80, GridUnitType.Pixel))
            ];
        roamingSettings.ServerSettings.ServerChanged += ServerSettings_ServerChanged;
    }

    private async void ServerSettings_ServerChanged(Model.Server server, Model.Settings.ServerSettingGroup.ServerOperation operation)
    {
        RefreshButton.IsEnabled = false;
        songList.Clear();
        foreach (var s in roamingSettings.ServerSettings.ActiveServers)
        {
            songList.AddRange(await SubsonicApiHelper.Search(s));
        }
        Refresh();
        initialized = true;
        RefreshButton.IsEnabled = true;
    }

    private CommandBarFlyout GridTable_RowRightTapped(object sender, Control.RowEvent e)
    {
        return SongCommandBarFlyout.Create(songList, songList[e.Index], GridTable, this, Model.Settings.BehaviorSettingGroup.GridTableDoubleClickBehavior.LoadCurrent);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshButton.IsEnabled = false;
        if (!initialized)
        {
            songList.Clear();
            foreach (var server in roamingSettings.ServerSettings.ActiveServers)
            {
                songList.AddRange(await SubsonicApiHelper.Search(server));
            }
            Refresh();
            initialized = true;
        }
        RefreshButton.IsEnabled = true;
    }

    private void GridTable_RowDoubleTapped(object sender, Control.RowEvent e)
    {
        SongCommandBarFlyout.PlayNow(new CommandBarFlyout(), songList[e.Index], songList, Model.Settings.BehaviorSettingGroup.GridTableDoubleClickBehavior.LoadCurrent);
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshButton.IsEnabled = false;
        songList.Clear();
        foreach (var server in roamingSettings.ServerSettings.ActiveServers)
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
