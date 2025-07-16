using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages.Control;

namespace WinSonic.Controls
{
    public class QueueSongCommandBarFlyout
    {
        public delegate void RemoveHandler();
        public static CommandBarFlyout Create(GridTable gridTable, uint index, MediaPlaybackList playlist)
        {
            var flyout = new CommandBarFlyout { AlwaysExpanded = true };

            var playButton = new AppBarButton
            {
                Label = "Play",
                Icon = new FontIcon { Glyph = "\uE768" }
            };
            playButton.Click += (_, _) => Play(playlist, index, flyout);

            var removeButton = new AppBarButton
            {
                Label = "Remove",
                Icon = new FontIcon { Glyph = "\uE738" }
            };
            removeButton.Click += (_, _) => Remove(index, gridTable, flyout);

            flyout.PrimaryCommands.Add(playButton);
            flyout.PrimaryCommands.Add(removeButton);

            return flyout;
        }

        private static void Play(MediaPlaybackList playlist, uint index, CommandBarFlyout flyout)
        {
            playlist.MoveTo(index);
            flyout.Hide();
        }

        private static void Remove(uint index, GridTable gridTable, CommandBarFlyout flyout)
        {
            bool currentSong = PlayerPlaylist.Instance.SongIndex == index;
            PlayerPlaylist.Instance.RemoveSong((int)index);
            gridTable.RemoveRow((int)index);
            gridTable.ShowContent();
            if (!currentSong)
            {
                var rect = gridTable.GetRectangle(PlayerPlaylist.Instance.SongIndex);
                gridTable.RectangleColors[rect] = true;
                rect.Fill = gridTable.Colors[true].Fill;
            }
            flyout.Hide();
        }
    }
}
