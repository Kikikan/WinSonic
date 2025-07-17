using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages;
using WinSonic.Pages.Dialog;
using WinSonic.ViewModel;

namespace WinSonic.Controls
{
    public class ArtistCommandBarFlyout
    {
        public static CommandBarFlyout Create(DetailedArtist artist, Page page, InfoWithPicture picture)
        {
            var flyout = new CommandBarFlyout { AlwaysExpanded = true };

            var playNowButton = new AppBarButton
            {
                Label = "Play Now",
                Icon = new FontIcon { Glyph = "\uE768" }
            };
            playNowButton.Click += (sender, e) => PlayNow(artist, flyout);

            var separator = new AppBarSeparator();

            var favouriteToggleButton = new AppBarToggleButton
            {
                Label = artist.IsFavourite ? "Unfavourite" : "Favourite",
                IsChecked = artist.IsFavourite,
                Icon = new FontIcon
                {
                    Glyph = artist.IsFavourite ? "\uEA92" : "\uEB51"
                }
            };
            favouriteToggleButton.Click += (sender, e) => Favourite(artist, picture, flyout);

            var addToPlaylistButton = new AppBarButton
            {
                Label = "Add to Playlist",
                Icon = new FontIcon { Glyph = "\uEA37" }
            };
            addToPlaylistButton.Click += async (sender, e) => await AddToPlaylist(artist, page, flyout);
            
            flyout.PrimaryCommands.Add(playNowButton);
            flyout.PrimaryCommands.Add(separator);
            flyout.PrimaryCommands.Add(favouriteToggleButton);
            flyout.PrimaryCommands.Add(addToPlaylistButton);

            return flyout;
        }

        public static async void PlayNow(DetailedArtist artist, CommandBarFlyout? flyout)
        {
            PlayerPlaylist.Instance.ClearSongs();
            var songs = await GetSongs(artist);
            foreach (var song in songs)
            {
                PlayerPlaylist.Instance.AddSong(song);
            }
            flyout?.Hide();
        }

        private static async Task<List<Song>> GetSongs(DetailedArtist artist)
        {
            var artistInfo = await SubsonicApiHelper.GetArtist(artist.Server, artist.Id);
            var albums = artistInfo.Album.Select(a => new Album(a, artist.Server)).ToList();

            var list = new List<Song>();
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
        public static async void Favourite(DetailedArtist artist, InfoWithPicture picture, CommandBarFlyout? flyout)
        {
            bool success = await SubsonicApiHelper.Star(artist.Server, !artist.IsFavourite, SubsonicApiHelper.StarType.Artist, artist.Id);
            if (success)
            {
                artist.IsFavourite = !artist.IsFavourite;
                picture.IsFavourite = !picture.IsFavourite;
            }
            flyout?.Hide();
        }

        public static async Task AddToPlaylist(DetailedArtist artist, Page page, CommandBarFlyout? flyout)
        {
            flyout?.Hide();
            var result = AddToPlaylistDialog.CreateDialog(page, artist, await GetSongs(artist));
            AddToPlaylistDialog.ProcessDialog(await result.Item1.ShowAsync(), result.Item2);
        }
    }
}
