using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages.Control;
using WinSonic.Pages.Dialog;

namespace WinSonic.Controls
{
    public class SongCommandBarFlyout
    {
        public delegate void SongEventHandler(CommandBarFlyout flyout, Song song);
        public delegate void PageSongEventHandler(CommandBarFlyout flyout, Song song, Page page, GridTable gridTable, List<Song> songs);
        public static CommandBarFlyout Create(
            List<Song> songs,
            Song song,
            GridTable gridTable,
            Page page)
        {
            var flyout = new CommandBarFlyout { AlwaysExpanded = true };

            var playNowButton = new AppBarButton
            {
                Label = "Play Now",
                Icon = new FontIcon { Glyph = "\uE768" }
            };
            playNowButton.Click += (sender, e) => PlayNow(flyout, song);

            var playNextButton = new AppBarButton
            {
                Label = "Play Next",
                Icon = new FontIcon { Glyph = "\uE893" }
            };
            playNextButton.Click += (sender, e) => PlayNext(flyout, song);

            var addToQueueButton = new AppBarButton
            {
                Label = "Add to Queue",
                Icon = new FontIcon { Glyph = "\uE710" }
            };
            addToQueueButton.Click += (sender, e) => AddToQueue(flyout, song);

            var separator = new AppBarSeparator();

            var favouriteToggleButton = new AppBarToggleButton
            {
                Label = song.IsFavourite ? "Unfavourite" : "Favourite",
                IsChecked = song.IsFavourite,
                Icon = new FontIcon
                {
                    Glyph = song.IsFavourite ? "\uEA92" : "\uEB51"
                }
            };
            favouriteToggleButton.Click += (sender, e) => Favourite(flyout, song, page, gridTable, songs);

            var addToPlaylistButton = new AppBarButton
            {
                Label = "Add to Playlist",
                Icon = new FontIcon { Glyph = "\uEA37" }
            };
            addToPlaylistButton.Click += (sender, e) => AddToPlaylist(flyout, song, page, gridTable, songs);

            flyout.PrimaryCommands.Add(playNowButton);
            flyout.PrimaryCommands.Add(playNextButton);
            flyout.PrimaryCommands.Add(addToQueueButton);
            flyout.PrimaryCommands.Add(separator);
            flyout.PrimaryCommands.Add(favouriteToggleButton);
            flyout.PrimaryCommands.Add(addToPlaylistButton);

            return flyout;
        }

        private static void PlayNow(CommandBarFlyout flyout, Song song)
        {
            PlayerPlaylist.Instance.ClearSongs();
            PlayerPlaylist.Instance.AddSong(song);
            flyout.Hide();
        }

        private static void PlayNext(CommandBarFlyout flyout, Song song)
        {
            PlayerPlaylist.Instance.AddSong(song, (int)((App)Application.Current).MediaPlaybackList.CurrentItemIndex + 1);
            flyout.Hide();
        }

        private static void AddToQueue(CommandBarFlyout flyout, Song song)
        {
            PlayerPlaylist.Instance.AddSong(song);
            flyout.Hide();
        }

        private static async void Favourite(CommandBarFlyout flyout, Song song, Page page, GridTable gridTable, List<Song> songs)
        {
            bool success = await SubsonicApiHelper.Star(song.Server, !song.IsFavourite, SubsonicApiHelper.StarType.Song, song.Id);
            if (success)
            {
                song.IsFavourite = !song.IsFavourite;
                var index = songs.IndexOf(song);
                if (index != -1)
                {
                    var rect = gridTable.GetRectangle(index);
                    gridTable.RectangleColors[rect] = song.IsFavourite;
                    rect.Fill = gridTable.Colors[song.IsFavourite].Fill;
                }
            }
            flyout.Hide();
        }

        private static async void AddToPlaylist(CommandBarFlyout flyout, Song song, Page page, GridTable gridTable, List<Song> songs)
        {
            flyout.Hide();
            var result = AddToPlaylistDialog.CreateDialog(page, song);
            AddToPlaylistDialog.ProcessDialog(await result.Item1.ShowAsync(), result.Item2);
        }
    }
}
