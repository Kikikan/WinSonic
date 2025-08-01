using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages.Dialog;
using WinSonic.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Details;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ArtistDetailPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public InfoWithPicture? DetailedObject { get; set; }
    private bool _initialized = false;
    public ArtistDetailPage()
    {
        InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.NavigationMode == NavigationMode.Back)
        {
            ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackFromAlbumArtistAnimation");
            imageAnimation?.TryStart(detailedImage, [coordinatedPanel]);
        }
        else
        {
            if (e.Parameter is InfoWithPicture info)
            {
                DetailedObject = info;

                ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("OpenPictureControlItemAnimation");
                imageAnimation?.TryStart(detailedImage, [coordinatedPanel]);
            }

        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        if (e.SourcePageType != typeof(AlbumDetailPage))
        {
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }
        else
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ArtistToAlbumAnimation", detailedImage);
        }
        if (e.NavigationMode == NavigationMode.Back)
        {
            if (e.SourcePageType == typeof(ArtistsPage))
            {
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ClosePictureControlItemAnimation", detailedImage);
            }
        }
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_initialized)
        {
            if (DetailedObject != null)
            {
                var artistInfo = await SubsonicApiHelper.GetArtist(DetailedObject.ApiObject.Server, DetailedObject.ApiObject.Id);
                foreach (var album in artistInfo.Album)
                {
                    Album albumObj = new(album, DetailedObject.ApiObject.Server);
                    var a = new InfoWithPicture(albumObj, albumObj.CoverImageUrl, albumObj.Title, albumObj.Artist, albumObj.IsFavourite, typeof(AlbumDetailPage), "", ((DetailedArtist)DetailedObject.ApiObject).SmallImageUri);
                    AlbumControl.Items.Add(a);
                }

                var artistInfo2 = await SubsonicApiHelper.GetArtistInfo(DetailedObject.ApiObject.Server, DetailedObject.ApiObject.Id);
                if (artistInfo2 != null)
                {
                    NoteTextBlock.Text = artistInfo2.Biography;
                }
            }
            _initialized = true;
        }
    }

    private async void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        PlayerPlaylist.Instance.ClearSongs();
        var songs = await GetSongs();
        foreach (var song in songs)
        {
            PlayerPlaylist.Instance.AddSong(song);
        }
    }

    private async Task<List<Song>> GetSongs()
    {
        var list = new List<Song>();
        var albums = AlbumControl.Items
            .Select(info => info.ApiObject)
            .Select(api => api as Album)
            .ToList();

        foreach (var album in albums)
        {
            if (album != null)
            {
                var rs = await SubsonicApiHelper.GetAlbum(album.Server, album.Id);
                if (rs != null)
                {
                    foreach (var child in rs.Song)
                    {
                        list.Add(new Song(child, album.Server));
                    }
                }
            }
        }
        return list;
    }

    private async void FavouriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (DetailedObject?.ApiObject is DetailedArtist artist)
        {
            bool success = await SubsonicApiHelper.Star(artist.Server, !artist.IsFavourite, SubsonicApiHelper.StarType.Artist, artist.Id);
            if (success)
            {
                DetailedObject.IsFavourite = !DetailedObject.IsFavourite;
                artist.IsFavourite = !artist.IsFavourite;
                OnPropertyChanged(nameof(DetailedObject));
            }
        }
    }

    private async void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
    {
        if (DetailedObject?.ApiObject is DetailedArtist artist)
        {
            var result = AddToPlaylistDialog.CreateDialog(this, artist, await GetSongs());
            AddToPlaylistDialog.ProcessDialog(await result.Item1.ShowAsync(), result.Item2);
        }
    }
}
