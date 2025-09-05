using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages.Dialog;
using WinSonic.ViewModel;

namespace WinSonic.Controls
{
    public class SongCollectionCommandBarFlyout
    {
        public static CommandBarFlyout Create(IFavourite obj, ApiObject apiObj, List<Song> songs, Page page, InfoWithPicture picture)
        {
            var flyout = new CommandBarFlyout { AlwaysExpanded = true };

            var playNowButton = new AppBarButton
            {
                Label = "Play Now",
                Icon = new FontIcon { Glyph = "\uE768" }
            };
            playNowButton.Click += (sender, e) => PlayNow(songs, flyout);

            var playNextButton = new AppBarButton
            {
                Label = "Play Next",
                Icon = new FontIcon { Glyph = "\uE893" }
            };
            playNextButton.Click += (sender, e) => PlayNext(songs, flyout);

            var addToQueueButton = new AppBarButton
            {
                Label = "Add to Queue",
                Icon = new FontIcon { Glyph = "\uE710" }
            };
            addToQueueButton.Click += (sender, e) => AddToQueue(songs, flyout);

            var separator = new AppBarSeparator();

            var favouriteToggleButton = new AppBarToggleButton
            {
                Label = obj.IsFavourite ? "Unfavourite" : "Favourite",
                IsChecked = obj.IsFavourite,
                Icon = new FontIcon
                {
                    Glyph = obj.IsFavourite ? "\uEA92" : "\uEB51"
                }
            };
            favouriteToggleButton.Click += (sender, e) => Favourite(obj, apiObj, picture, flyout);

            var addToPlaylistButton = new AppBarButton
            {
                Label = "Add to Playlist",
                Icon = new FontIcon { Glyph = "\uEA37" }
            };
            addToPlaylistButton.Click += async (sender, e) => await AddToPlaylist(songs, page, flyout);

            flyout.PrimaryCommands.Add(playNowButton);
            flyout.PrimaryCommands.Add(playNextButton);
            flyout.PrimaryCommands.Add(addToQueueButton);
            flyout.PrimaryCommands.Add(separator);
            flyout.PrimaryCommands.Add(favouriteToggleButton);
            flyout.PrimaryCommands.Add(addToPlaylistButton);

            return flyout;
        }

        public static void PlayNow(List<Song> songs, CommandBarFlyout? flyout)
        {
            PlayerPlaylist.Instance.ClearSongs();
            AddToQueue(songs, flyout);
        }

        public static void PlayNext(List<Song> songs, CommandBarFlyout? flyout)
        {
            for (int i = songs.Count - 1; i >= 0; i--)
            {
                PlayerPlaylist.Instance.AddNextSong(songs[i]);
            }
            flyout?.Hide();
        }

        public static void AddToQueue(List<Song> songs, CommandBarFlyout? flyout)
        {
            foreach (var song in songs)
            {
                PlayerPlaylist.Instance.AddSong(song);
            }
            flyout?.Hide();
        }

        private static async void Favourite(IFavourite obj, ApiObject apiObj, InfoWithPicture picture, CommandBarFlyout? flyout)
        {
            bool success = await SubsonicApiHelper.Star(apiObj.Server, !obj.IsFavourite, obj.Type, apiObj.Id);
            if (success)
            {
                obj.IsFavourite = !obj.IsFavourite;
                picture.IsFavourite = !picture.IsFavourite;
            }
            flyout?.Hide();
        }

        public static async Task AddToPlaylist(List<Song> songs, Page page, CommandBarFlyout? flyout)
        {
            flyout?.Hide();
            var result = AddToPlaylistDialog.CreateDialog(page, songs);
            AddToPlaylistDialog.ProcessDialog(await result.Item1.ShowAsync(), result.Item2);
        }
    }
}
