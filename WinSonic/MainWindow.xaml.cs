using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Windows.Media.Core;
using WinSonic.Model;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages;
using WinSonic.Pages.Details;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static readonly List<Type> BackAllowedPages = [typeof(AlbumDetailPage), typeof(ArtistDetailPage), typeof(PlayerPage)];
        public Frame NavFrame { get { return ContentFrame; } }
        private Song? _song;
        private bool _isLoading = true;
        private bool IsLoading { get => _isLoading; set { _isLoading = value; LoadingOverlay.Visibility = value ? Visibility.Visible : Visibility.Collapsed; } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Song? Song { get => _song; set { _song = value; OnPropertyChanged(nameof(Song)); } }
        public ICommand ShowWindowCommand { get; }
        public ICommand? CancelCloseCommand { get; }
        public ICommand ExitApplicationCommand { get; }

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            this.Closed += OnWindowClosing;

            ShowWindowCommand = new RelayCommand(ShowWindow);
            ExitApplicationCommand = new RelayCommand(ExitApplication);

            PlayerPlaylist.Instance.SongAdded += Playlist_SongAdded;
            PlayerPlaylist.Instance.SongRemoved += Playlist_SongRemoved;
            PlayerPlaylist.Instance.SongIndexChanged += Playlist_SongIndexChanged;
            MediaPlayerElement.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ShowUnsuccessfulServers(await ((App)Application.Current).ServerFile.Initialize());
        }

        private async void ShowUnsuccessfulServers(List<Server> servers)
        {
            if (servers != null && servers.Count > 0)
            {
                ContentDialog dialog = new()
                {
                    XamlRoot = MainGrid.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Unsuccessful Connection",
                    PrimaryButtonText = "Retry for selected",
                    CloseButtonText = "Close",
                    IsSecondaryButtonEnabled = false,
                    DefaultButton = ContentDialogButton.Primary
                };
                var content = new UnsuccessfulConnectionDialog();
                foreach (var server in servers)
                {
                    content.Servers.Add(server);
                }
                dialog.Content = content;

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary && content.ServerListView != null)
                {
                    List<Server> attemptList = [];
                    foreach (var obj in content.ServerListView.SelectedItems)
                    {
                        if (obj is Server server)
                        {
                            attemptList.Add(server);
                        }
                    }
                    List<Server> attemptResult = await ((App)Application.Current).ServerFile.TryPing(attemptList);
                    ShowUnsuccessfulServers(attemptResult);
                } else
                {
                    IsLoading = false;
                }
            }
            else
            {
                IsLoading = false;
            }
        }

        private void Playlist_SongIndexChanged(object? sender, int oldIndex)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                MediaPlayerElement.Source = null;
                StartSong();
                Song = PlayerPlaylist.Instance.Song;
            });
        }

        private void MediaPlayer_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            PlayerPlaylist.Instance.SongIndex++;
        }

        private void Playlist_SongRemoved(object? sender, int index)
        {
            if (PlayerPlaylist.Instance.SongIndex == index)
            {
                MediaPlayerElement.Source = null;
            }
            StartSong();
        }

        private void Playlist_SongAdded(object? sender, Song song)
        {
            StartSong();
        }

        private async void StartSong()
        {
            if (MediaPlayerElement.Source == null && PlayerPlaylist.Instance.Song != null)
            {
                await SubsonicApiHelper.Scrobble(PlayerPlaylist.Instance.Song.Server, PlayerPlaylist.Instance.Song.Id);
                MediaPlayerElement.Source = MediaSource.CreateFromUri(PlayerPlaylist.Instance.Song.StreamUri);
            }
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                NavView_Navigate(sender, typeof(SettingsPage), args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null && args.InvokedItemContainer.Tag != null)
            {
                string? tagString = args.InvokedItemContainer.Tag.ToString();
                if (tagString != null)
                {
                    Type? navPageType = Type.GetType(tagString);
                    if (navPageType != null)
                    {
                        NavView_Navigate(sender, navPageType, args.RecommendedNavigationTransitionInfo);
                    }
                }

            }
        }

        private void NavView_Navigate(NavigationView sender, Type navPageType, NavigationTransitionInfo transitionInfo)
        {
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (navPageType is not null && !Type.Equals(preNavPageType, navPageType))
            {
                ContentFrame.Navigate(navPageType, null, transitionInfo);
            }
        }

        private void MainNav_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (NavFrame.CanGoBack && BackAllowedPages.Contains(NavFrame.CurrentSourcePageType))
            {
                NavFrame.GoBack();
            }
        }
        public void ShowFromTray()
        {
            this.Show();
            this.Activate();
        }
        private void OnWindowClosing(object sender, WindowEventArgs args)
        {
            // Prevent window from closing
            args.Handled = true;

            // Hide instead (go to tray)
            this.Hide();
        }
        private void ShowWindow()
        {
            this.ShowFromTray();
        }

        private void ExitApplication()
        {
            TrayIcon.Dispose();
            Environment.Exit(0);
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Only play reverse animation when coming back from CurrentlyPlayingPage
            if (e.SourcePageType != typeof(PlayerPage)) // or whatever your main page is
            {
                SongInfoStackPanel.Visibility = Visibility.Visible;
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("coverImageBackAnimation");

                    if (animation != null)
                    {
                        animation.Configuration = new DirectConnectedAnimationConfiguration();
                        animation.TryStart(CoverImage); // CoverImage in MainWindow
                    }
                });
            }
        }

        private void SongInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var animationService = ConnectedAnimationService.GetForCurrentView();

            // Start animation from the image (you can use any element)
            animationService.PrepareToAnimate("coverImageAnimation", CoverImage); // CoverImage = your Image control

            // Navigate to the page in the NavigationView Frame
            NavFrame.Navigate(typeof(PlayerPage));
            SongInfoStackPanel.Visibility = Visibility.Collapsed;
        }
    }
    // Simple RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
