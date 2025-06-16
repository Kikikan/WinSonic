using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Windows.Media.Playback;
using WinSonic.Model;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages;
using WinSonic.Pages.Details;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static MainWindow? Instance { get; private set; } // Singleton for easy access

        private static readonly List<Type> BackAllowedPages = [typeof(AlbumDetailPage), typeof(ArtistDetailPage), typeof(PlayerPage)];
        public Frame NavFrame { get { return ContentFrame; } }
        private bool _isLoading = true;
        private bool IsLoading { get => _isLoading; set { _isLoading = value; LoadingOverlay.Visibility = value ? Visibility.Visible : Visibility.Collapsed; } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            DispatcherQueue.TryEnqueue(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));

        public Song? Song { get; private set; }
        private readonly MediaPlaybackList MediaPlaybackList;
        public ICommand ShowWindowCommand { get; }
        public ICommand? CancelCloseCommand { get; }
        public ICommand ExitApplicationCommand { get; }
        public ICommand ShowMiniPlayerCommand => new RelayCommand(ShowMiniPlayerFlyout);
        private MiniPlayerWindow? _miniPlayerWindow;

        public MediaPlayer SharedMediaPlayer => MediaPlayerElement.MediaPlayer; // Expose MediaPlayer

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            this.Closed += OnWindowClosing;

            ShowWindowCommand = new RelayCommand(ShowWindow);
            ExitApplicationCommand = new RelayCommand(ExitApplication);

            if (Application.Current is App app)
            {
                MediaPlayerElement.SetMediaPlayer(app.MediaPlayer);
                MediaPlaybackList = app.MediaPlaybackList;
                MediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            }
            else
            {
                throw new Exception("Application is not an App.");
            }
            AppWindow.Resize(new Windows.Graphics.SizeInt32(975, 900));
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

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ShowUnsuccessfulServers(await ((App)Application.Current).RoamingSettings.InitializeServers());
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
                    List<Server> attemptResult = await RoamingSettings.TryPing(attemptList);
                    ShowUnsuccessfulServers(attemptResult);
                }
                else
                {
                    IsLoading = false;
                }
            }
            else
            {
                IsLoading = false;
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
            Type preNavPageType = ContentFrame.CurrentSourcePageType;

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
            args.Handled = true;
            this.Hide();
        }
        private void ShowWindow()
        {
            this.ShowFromTray();
        }

        private void ShowMiniPlayerFlyout()
        {
            if (_miniPlayerWindow == null)
            {
                _miniPlayerWindow = new MiniPlayerWindow();
                _miniPlayerWindow.Closed += (s, e) => _miniPlayerWindow = null;
                _miniPlayerWindow.Activate();
            }
            else
            {
                _miniPlayerWindow.Activate();
            }
        }

        private void ExitApplication()
        {
            TrayIcon.Dispose();
            Environment.Exit(0);
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType != typeof(PlayerPage))
            {
                SongInfoStackPanel.Visibility = Visibility.Visible;
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("coverImageBackAnimation");

                    if (animation != null)
                    {
                        animation.Configuration = new DirectConnectedAnimationConfiguration();
                        animation.TryStart(CoverImage);
                    }
                });
            }
        }

        private void SongInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var animationService = ConnectedAnimationService.GetForCurrentView();
            animationService.PrepareToAnimate("coverImageAnimation", CoverImage);
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
