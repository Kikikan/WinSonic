using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private int previousSelectedIndex = -1;

        private Song? _song;
        private Song? Song { get => _song; set { _song = value; DispatcherQueue.TryEnqueue(() => OnPropertyChanged(nameof(Song))); } }

        public PlayerPage()
        {
            InitializeComponent();
            PlayerPlaylist.Instance.SongIndexChanged += Playlist_SongIndexChanged;
            Song = PlayerPlaylist.Instance.Song;
        }

        private void Playlist_SongIndexChanged(object? sender, int oldIndex)
        {
            Song = PlayerPlaylist.Instance.Song;
        }

        private void RightSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            System.Type pageType;

            switch (currentSelectedIndex)
            {
                case 0:
                    pageType = typeof(QueuePage);
                    break;
                case 1:
                    pageType = typeof(LyricsPage);
                    break;
                default:
                    pageType = typeof(RelatedPage);
                    break;
            }

            var slideNavigationTransitionEffect = currentSelectedIndex - previousSelectedIndex > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;

            ContentFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = slideNavigationTransitionEffect });

            previousSelectedIndex = currentSelectedIndex;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Wait for the UI to load before trying to animate
            this.Loaded += CurrentlyPlayingPage_Loaded;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.Back)
            {
                var animationService = ConnectedAnimationService.GetForCurrentView();
                animationService.PrepareToAnimate("coverImageBackAnimation", Image);
            }
        }

        private void CurrentlyPlayingPage_Loaded(object sender, RoutedEventArgs e)
        {
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("coverImageAnimation");

            if (animation != null)
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
                animation.TryStart(Image);
            }
        }
    }
}
