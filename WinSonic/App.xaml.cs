using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        internal ServerFile ServerFile { get; private set; } = new();

        public MainWindow? Window { get; private set; }
        public MediaPlayer MediaPlayer { get; private set; } = new();
        public MediaPlaybackList MediaPlaybackList { get; private set; } = new();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            MediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            PlayerPlaylist.Instance.SongAdded += Instance_SongAdded;
            PlayerPlaylist.Instance.SongRemoved += Instance_SongRemoved;
        }

        private async void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (MediaPlaybackList.CurrentItem != null)
            {
                var song = PlayerPlaylist.Instance.Songs[(int)MediaPlaybackList.CurrentItemIndex];
                await SubsonicApiHelper.Scrobble(song.Server, song.Id);
            }
        }

        private void Instance_SongRemoved(object? sender, int index)
        {
            MediaPlaybackList.Items.RemoveAt(index);
        }

        private void Instance_SongAdded(object? sender, Song song)
        {
            MediaPlaybackList.Items.Add(CreatePlaybackItem(song));
            if (MediaPlayer.Source == null)
            {
                MediaPlayer.Source = MediaPlaybackList;
                MediaPlayer.Play();
            }
        }

        private static MediaPlaybackItem CreatePlaybackItem(Song song)
        {
            var mediaSource = MediaSource.CreateFromUri(song.StreamUri);
            var playbackItem = new MediaPlaybackItem(mediaSource);

            var props = playbackItem.GetDisplayProperties();
            props.Type = MediaPlaybackType.Music;
            props.MusicProperties.Title = song.Title;
            props.MusicProperties.Artist = song.Artist;

            if (song.CoverImageUri != null)
            {
                props.Thumbnail = RandomAccessStreamReference.CreateFromUri(song.CoverImageUri);
            }

            playbackItem.ApplyDisplayProperties(props);
            return playbackItem;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Window = new MainWindow();
            Window.Activate();
            ConfigureSystemMediaTransportControls();
        }

        private void ConfigureSystemMediaTransportControls()
        {
            var smtc = MediaPlayer.SystemMediaTransportControls;
            smtc.IsEnabled = true;
            smtc.IsPlayEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;
            MediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
        }
    }
}
