using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.Media.Core;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public Frame NavFrame { get { return ContentFrame; } }
        public ICommand ShowWindowCommand { get; }
        public ICommand CancelCloseCommand { get; }
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

        private void Playlist_SongIndexChanged(object? sender, int oldIndex)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                MediaPlayerElement.Source = null;
                StartSong();
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
                Type navPageType = Type.GetType(args.InvokedItemContainer.Tag.ToString());
                NavView_Navigate(sender, navPageType, args.RecommendedNavigationTransitionInfo);
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
            if (NavFrame.CanGoBack)
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
    }
    // Simple RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly System.Action _execute;
        private readonly System.Func<bool> _canExecute;

        public RelayCommand(System.Action execute, System.Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new System.ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event System.EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
