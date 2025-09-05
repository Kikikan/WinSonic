using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private bool IsFiltered() => FavouritesFilterCheckBox.IsChecked;
    private readonly RoamingSettings roamingSettings = ((App)Application.Current).RoamingSettings;
    private readonly List<Song> songList = [];
    private readonly List<Song> shownSongs = [];
    private bool initialized = false;
    private string? id = null;

    public SongsPage()
    {
        InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
        GridTable.Columns = [
            ("Title", new GridLength(4, GridUnitType.Star)),
            ("Artist", new GridLength(2, GridUnitType.Star)),
            ("Album", new GridLength(3, GridUnitType.Star)),
            ("Time", new GridLength(80, GridUnitType.Pixel))
            ];
        roamingSettings.ServerSettings.ServerChanged += ServerSettings_ServerChanged;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string id)
        {
            this.id = id;
        }
        else
        {
            this.id = null;
        }
    }

    private async void ServerSettings_ServerChanged(Model.Server server, Model.Settings.ServerSettingGroup.ServerOperation operation)
    {
        await RefreshSongs();
        initialized = true;
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
            await RefreshSongs();
            initialized = true;
        }
        RefreshButton.IsEnabled = true;
        if (id != null)
        {
            await SelectSong(id);
        }
    }

    private void GridTable_RowDoubleTapped(object sender, Control.RowEvent e)
    {
        SongCommandBarFlyout.PlayNow(new CommandBarFlyout(), songList[e.Index], songList, Model.Settings.BehaviorSettingGroup.GridTableDoubleClickBehavior.LoadCurrent);
    }

    private void ClearFilters()
    {
        FavouritesFilterCheckBox.IsChecked = false;
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshSongs();
    }

    private async Task RefreshSongs()
    {
        RefreshButton.IsEnabled = false;
        songList.Clear();
        foreach (var server in roamingSettings.ServerSettings.ActiveServers)
        {
            songList.AddRange((await SubsonicApiHelper.Search(server)).Item1);
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

    public async Task SelectSong(string id)
    {
        if (IsFiltered())
        {
            ClearFilters();
            await RefreshSongs();
        }
        for (int index = 0; index < songList.Count; index++)
        {
            if (songList[index].Id == id)
            {
                GridTable.SelectedIndex = index;
                break;
            }
        }
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
