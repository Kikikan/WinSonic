using Microsoft.UI.Xaml;
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
        internal RoamingSettings RoamingSettings { get; private set; } = new();

        public MainWindow? Window { get; private set; }
        public MediaPlayer MediaPlayer { get; private set; } = new();
        public MediaPlaybackList MediaPlaybackList { get; private set; } = new();

        private bool autoPlayNext = false;

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
            PlayerPlaylist.Instance.SongIndexChanged += Instance_SongIndexChanged;
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            MediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
            MediaPlayer.Volume = RoamingSettings.PlayerSettings.Volume;
        }

        private void MediaPlayer_VolumeChanged(MediaPlayer sender, object args)
        {
            RoamingSettings.PlayerSettings.Volume = sender.Volume;
            RoamingSettings.SaveSetting(RoamingSettings.PlayerSettings);
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            autoPlayNext = true;
        }

        private async void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (MediaPlaybackList.CurrentItem != null)
            {
                PlayerPlaylist.Instance.SongIndex = (int)MediaPlaybackList.CurrentItemIndex;
                var song = PlayerPlaylist.Instance.Songs[PlayerPlaylist.Instance.SongIndex];
                await SubsonicApiHelper.Scrobble(song.Server, song.Id);
            }
        }

        private void Instance_SongRemoved(object? sender, int index)
        {
            if (MediaPlaybackList.CurrentItemIndex == index)
            {
                if (index < MediaPlaybackList.Items.Count - 1)
                {
                    MediaPlaybackList.MoveNext();
                }
                else
                {
                    MediaPlaybackList.MoveTo(0);
                    Window?.DispatcherQueue.TryEnqueue(() => MediaPlayer.Pause());
                }
            }
            MediaPlaybackList.Items.RemoveAt(index);
            if (MediaPlaybackList.Items.Count == 0)
            {
                autoPlayNext = true;
            }
        }

        private void Instance_SongAdded(object? sender, Song song, int index)
        {
            MediaPlaybackList.Items.Insert(index, CreatePlaybackItem(song));
            if (MediaPlayer.Source == null)
            {
                MediaPlayer.Source = MediaPlaybackList;
                Window?.DispatcherQueue.TryEnqueue(() => MediaPlayer.Play());
            }
            if (autoPlayNext)
            {
                autoPlayNext = false;
                MediaPlaybackList.MoveTo((uint)index);
                Window?.DispatcherQueue.TryEnqueue(() => MediaPlayer.Play());
            }
        }

        private void Instance_SongIndexChanged(object? sender, int index)
        {
            MediaPlaybackList.MoveTo((uint)index);
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
