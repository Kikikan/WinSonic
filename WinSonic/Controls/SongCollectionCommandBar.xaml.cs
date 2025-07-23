using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinSonic.Model.Api;
using WinSonic.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Controls;

public sealed partial class SongCollectionCommandBar : UserControl
{
    public IFavourite? FavObj { get; set; }
    public ApiObject? ApiObj { get; set; }
    public InfoWithPicture? ShownObj { get; set; }
    public bool IsFavourite { get; set; }
    public List<Song> Songs { get; set; } = [];

    public event EmptySongsHandler? EmptySongs;
    public delegate Task<List<Song>> EmptySongsHandler();
    public SongCollectionCommandBar()
    {
        InitializeComponent();
    }
    private async void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        await CheckSongs();
        SongCollectionCommandBarFlyout.PlayNow(Songs, null);
    }

    private async void PlayNextButton_Click(object sender, RoutedEventArgs e)
    {
        await CheckSongs();
        SongCollectionCommandBarFlyout.PlayNext(Songs, null);
    }

    private async void AddToQueueButton_Click(object sender, RoutedEventArgs e)
    {
        await CheckSongs();
        SongCollectionCommandBarFlyout.AddToQueue(Songs, null);
    }

    private async void FavouriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (ApiObj != null && FavObj != null)
        {
            bool success = await SubsonicApiHelper.Star(ApiObj.Server, !FavObj.IsFavourite, FavObj.Type, ApiObj.Id);
            if (success)
            {
                if (ShownObj != null)
                {
                    ShownObj.IsFavourite = !ShownObj.IsFavourite;
                }
                FavObj.IsFavourite = !FavObj.IsFavourite;
//                OnPropertyChanged(nameof(DetailedObject));
            }
        }
    }

    private async void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
    {
        await CheckSongs();
        await SongCollectionCommandBarFlyout.AddToPlaylist(Songs, GetContainingPage(this), null);
    }

    private static Page GetContainingPage(DependencyObject? obj)
    {
        while (obj != null)
        {
            if (obj is Page page)
                return page;

            obj = VisualTreeHelper.GetParent(obj);
        }

        throw new Exception("Page not found.");
    }

    private async Task CheckSongs()
    {
        if (Songs.Count == 0 && EmptySongs != null)
        {
            Songs = await EmptySongs.Invoke();
        }
    }
}
