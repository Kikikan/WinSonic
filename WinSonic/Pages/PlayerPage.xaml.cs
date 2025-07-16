using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.ComponentModel;
using Windows.Media.Playback;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages.Player;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayerPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            DispatcherQueue.TryEnqueue(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));

        private Song? Song;

        private int previousSelectedIndex = -1;
        private readonly MediaPlaybackList MediaPlaybackList;

        public PlayerPage()
        {
            InitializeComponent();
            if (Application.Current is App app)
            {
                MediaPlaybackList = app.MediaPlaybackList;
                MediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
                if (app.MediaPlaybackList.CurrentItem != null)
                {
                    Song = PlayerPlaylist.Instance.Songs[(int)app.MediaPlaybackList.CurrentItemIndex];
                    OnPropertyChanged(nameof(Song));
                }
            }
            else
            {
                throw new Exception("Application is not an App.");
            }
        }

        private void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (MediaPlaybackList.CurrentItemIndex < PlayerPlaylist.Instance.Songs.Count)
            {
                Song = PlayerPlaylist.Instance.Songs[(int)MediaPlaybackList.CurrentItemIndex];
            }
            else
            {
                Song = null;
            }
            OnPropertyChanged(nameof(Song));
        }

        private void RightSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            Type pageType = currentSelectedIndex switch
            {
                0 => typeof(QueuePage),
                1 => typeof(LyricsPage),
                _ => typeof(RelatedPage),
            };
            var slideNavigationTransitionEffect = currentSelectedIndex - previousSelectedIndex > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;

            ContentFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = slideNavigationTransitionEffect });

            previousSelectedIndex = currentSelectedIndex;

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.Back)
            {
                var animationService = ConnectedAnimationService.GetForCurrentView();
                animationService.PrepareToAnimate("coverImageBackAnimation", CurrentSongPanel);
            }
        }

        private void CurrentlyPlayingPage_Loaded(object sender, RoutedEventArgs e)
        {
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("coverImageAnimation");

            if (animation != null)
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
                animation.TryStart(CurrentSongPanel);
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Image.MaxWidth = e.NewSize.Height - 100;
        }
    }
}
