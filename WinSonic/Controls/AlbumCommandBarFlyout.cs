using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages.Dialog;
using WinSonic.ViewModel;

namespace WinSonic.Controls
{
    public class AlbumCommandBarFlyout
    {
        public static CommandBarFlyout Create(Album album, Page page, InfoWithPicture picture)
        {
            var flyout = new CommandBarFlyout { AlwaysExpanded = true };

            var playNowButton = new AppBarButton
            {
                Label = "Play Now",
                Icon = new FontIcon { Glyph = "\uE768" }
            };
            playNowButton.Click += (sender, e) => PlayNow(album, flyout);

            var playNextButton = new AppBarButton
            {
                Label = "Play Next",
                Icon = new FontIcon { Glyph = "\uE893" }
            };
            playNextButton.Click += (sender, e) => PlayNext(album, flyout);

            var addToQueueButton = new AppBarButton
            {
                Label = "Add to Queue",
                Icon = new FontIcon { Glyph = "\uE710" }
            };
            addToQueueButton.Click += (sender, e) => AddToQueue(album, flyout);

            var separator = new AppBarSeparator();

            var favouriteToggleButton = new AppBarToggleButton
            {
                Label = album.IsFavourite ? "Unfavourite" : "Favourite",
                IsChecked = album.IsFavourite,
                Icon = new FontIcon
                {
                    Glyph = album.IsFavourite ? "\uEA92" : "\uEB51"
                }
            };
            favouriteToggleButton.Click += (sender, e) => Favourite(album, picture, flyout);

            var addToPlaylistButton = new AppBarButton
            {
                Label = "Add to Playlist",
                Icon = new FontIcon { Glyph = "\uEA37" }
            };
            addToPlaylistButton.Click += async (sender, e) => await AddToPlaylist(album, page, flyout);

            flyout.PrimaryCommands.Add(playNowButton);
            flyout.PrimaryCommands.Add(playNextButton);
            flyout.PrimaryCommands.Add(addToQueueButton);
            flyout.PrimaryCommands.Add(separator);
            flyout.PrimaryCommands.Add(favouriteToggleButton);
            flyout.PrimaryCommands.Add(addToPlaylistButton);

            return flyout;
        }

        public static void PlayNow(Album album, CommandBarFlyout? flyout)
        {
            PlayerPlaylist.Instance.ClearSongs();
            AddToQueue(album, flyout);
        }

        public static async void PlayNext(Album album, CommandBarFlyout? flyout)
        {
            var songs = await SubsonicApiHelper.GetSongs(album);
            for (int i = songs.Count - 1; i >= 0; i--)
            {
                PlayerPlaylist.Instance.AddNextSong(songs[i]);
            }
            flyout?.Hide();
        }

        public static async void AddToQueue(Album album, CommandBarFlyout? flyout)
        {
            var songs = await SubsonicApiHelper.GetSongs(album);
            foreach (var song in songs)
            {
                PlayerPlaylist.Instance.AddSong(song);
            }
            flyout?.Hide();
        }

        private static async void Favourite(Album album, InfoWithPicture picture, CommandBarFlyout? flyout)
        {
            bool success = await SubsonicApiHelper.Star(album.Server, !album.IsFavourite, SubsonicApiHelper.StarType.Album, album.Id);
            if (success)
            {
                album.IsFavourite = !album.IsFavourite;
                picture.IsFavourite = !picture.IsFavourite;
            }
            flyout?.Hide();
        }

        public static async Task AddToPlaylist(Album album, Page page, CommandBarFlyout? flyout)
        {
            flyout?.Hide();
            var songs = await SubsonicApiHelper.GetSongs(album);
            var result = AddToPlaylistDialog.CreateDialog(page, album, songs);
            AddToPlaylistDialog.ProcessDialog(await result.Item1.ShowAsync(), result.Item2);
        }
    }
}
