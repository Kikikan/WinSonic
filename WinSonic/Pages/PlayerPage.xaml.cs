using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
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
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private int previousSelectedIndex = -1;

        private Song _song = PlayerPlaylist.Instance.Song;
        private Song Song { get => _song; set { _song = value; OnPropertyChanged(nameof(Song)); } }
        public ObservableCollection<Song> Songs { get; set; } = new();
        public PlayerPage()
        {
            InitializeComponent();
            PlayerPlaylist.Instance.SongIndexChanged += Playlist_SongIndexChanged;
        }

        private void Playlist_SongIndexChanged(object? sender, int oldIndex)
        {
            Song = PlayerPlaylist.Instance.Song;
            // TODO: Fix view not updating
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

        private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            foreach (var song in PlayerPlaylist.Instance.Songs)
            {
                Songs.Add(song);
            }
        }

        private void PlaylistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlayerPlaylist.Instance.SongIndex = Songs.IndexOf(e.ClickedItem as Song);
        }

        private void PlaylistView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            PlayerPlaylist.Instance.Songs = PlaylistView.Items.Cast<Song>().ToList();
        }
    }
}
