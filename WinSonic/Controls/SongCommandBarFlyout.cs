using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSonic.Model.Api;

namespace WinSonic.Controls
{
    public class SongCommandBarFlyout
    {
        public static CommandBarFlyout Create(
        Song song,
        RoutedEventHandler playNowClick,
        RoutedEventHandler playNextClick,
        RoutedEventHandler addToQueueClick,
        RoutedEventHandler favouriteClick,
        RoutedEventHandler addToPlaylistClick)
        {
            var flyout = new CommandBarFlyout { AlwaysExpanded = true };

            // Play Now
            var playNowButton = new AppBarButton
            {
                Label = "Play Now",
                Icon = new FontIcon { Glyph = "\uE768" }
            };
            playNowButton.Click += playNowClick;

            // Play Next
            var playNextButton = new AppBarButton
            {
                Label = "Play Next",
                Icon = new FontIcon { Glyph = "\uE893" }
            };
            playNextButton.Click += playNextClick;

            // Add to Queue
            var addToQueueButton = new AppBarButton
            {
                Label = "Add to Queue",
                Icon = new FontIcon { Glyph = "\uE710" }
            };
            addToQueueButton.Click += addToQueueClick;

            // Separator
            var separator = new AppBarSeparator();

            // Favourite Toggle
            var favouriteToggleButton = new AppBarToggleButton
            {
                Label = song.IsFavourite ? "Unfavourite" : "Favourite",
                IsChecked = song.IsFavourite,
                Icon = new FontIcon
                {
                    Glyph = song.IsFavourite ? "\uEA92" : "\uEB51"
                }
            };
            favouriteToggleButton.Click += favouriteClick;

            // Add to Playlist
            var addToPlaylistButton = new AppBarButton
            {
                Label = "Add to Playlist",
                Icon = new FontIcon { Glyph = "\uEA37" }
            };
            addToPlaylistButton.Click += addToPlaylistClick;

            // Add everything to the flyout
            flyout.PrimaryCommands.Add(playNowButton);
            flyout.PrimaryCommands.Add(playNextButton);
            flyout.PrimaryCommands.Add(addToQueueButton);
            flyout.PrimaryCommands.Add(separator);
            flyout.PrimaryCommands.Add(favouriteToggleButton);
            flyout.PrimaryCommands.Add(addToPlaylistButton);

            return flyout;
        }
    }
}
