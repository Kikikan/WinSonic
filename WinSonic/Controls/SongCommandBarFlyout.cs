using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinSonic.Model.Api;

namespace WinSonic.Controls
{
    public class SongCommandBarFlyout
    {
        public delegate void SongEventHandler(object sender, RoutedEventArgs e, Song song);
        public static CommandBarFlyout Create(
        Song song,
        SongEventHandler playNowClick,
        SongEventHandler playNextClick,
        SongEventHandler addToQueueClick,
        SongEventHandler favouriteClick,
        SongEventHandler addToPlaylistClick)
        {
            var flyout = new CommandBarFlyout { AlwaysExpanded = true };

            var playNowButton = new AppBarButton
            {
                Label = "Play Now",
                Icon = new FontIcon { Glyph = "\uE768" }
            };
            playNowButton.Click += (sender, e) => playNowClick(sender, e, song);

            var playNextButton = new AppBarButton
            {
                Label = "Play Next",
                Icon = new FontIcon { Glyph = "\uE893" }
            };
            playNextButton.Click += (sender, e) => playNextClick(sender, e, song);

            var addToQueueButton = new AppBarButton
            {
                Label = "Add to Queue",
                Icon = new FontIcon { Glyph = "\uE710" }
            };
            addToQueueButton.Click += (sender, e) => addToQueueClick(sender, e, song);

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
            favouriteToggleButton.Click += (sender, e) => favouriteClick(sender, e, song);

            var addToPlaylistButton = new AppBarButton
            {
                Label = "Add to Playlist",
                Icon = new FontIcon { Glyph = "\uEA37" }
            };
            addToPlaylistButton.Click += (sender, e) => addToPlaylistClick(sender, e, song);

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
