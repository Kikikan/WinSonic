using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Model.Settings;
using WinSonic.Pages.Control;
using WinSonic.Pages.Dialog;

namespace WinSonic.Controls
{
    public class SongCommandBarFlyout
    {
        public static CommandBarFlyout Create(
            List<Song> songs,
            Song song,
            GridTable gridTable,
            Page page,
            BehaviorSettings.GridTableDoubleClickBehavior behavior)
        {
            var flyout = new CommandBarFlyout { AlwaysExpanded = true };

            var playNowButton = new AppBarButton
            {
                Label = "Play Now",
                Icon = new FontIcon { Glyph = "\uE768" }
            };
            playNowButton.Click += (sender, e) => PlayNow(flyout, song, songs, behavior);

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

        public static void PlayNow(CommandBarFlyout flyout, Song song, List<Song> songs, BehaviorSettings.GridTableDoubleClickBehavior behavior)
        {
            PlayerPlaylist.Instance.ClearSongs();
            if (behavior == BehaviorSettings.GridTableDoubleClickBehavior.LoadCurrent)
            {
                PlayerPlaylist.Instance.AddSong(song);
            }
            else if (behavior == BehaviorSettings.GridTableDoubleClickBehavior.LoadAll)
            {
                foreach (var s in songs)
                {
                    PlayerPlaylist.Instance.AddSong(s);
                }
                PlayerPlaylist.Instance.PlaySong(songs.IndexOf(song));
            }
            else
            {
                throw new ArgumentException("Unexpected GridTableDoubleClickBehavior value.");
            }
            flyout.Hide();
        }

        private static void PlayNext(CommandBarFlyout flyout, Song song)
        {
            PlayerPlaylist.Instance.AddNextSong(song);
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
