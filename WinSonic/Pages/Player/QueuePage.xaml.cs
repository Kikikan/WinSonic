using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Windows.Media.Playback;
using WinSonic.Controls;
using WinSonic.Model.Api;
using WinSonic.Model.Player;
using WinSonic.Pages.Control;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Player
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QueuePage : Page
    {
        public ObservableCollection<Song> Songs { get; set; } = [];
        private readonly App app;
        private readonly MediaPlaybackList Queue;
        private Rectangle? PreviousSongRectangle;
        private string RepeatGlyph { get { return GetRepeatGlyph(); } }
        private bool? RepeatChecked { get { return GetRepeatChecked(); } }

        public QueuePage()
        {
            InitializeComponent();
            if (Application.Current is App app)
            {
                this.app = app;
                Queue = app.MediaPlaybackList;
                Queue.CurrentItemChanged += Queue_CurrentItemChanged;
            }
            else
            {
                throw new Exception("Application is not App.");
            }
            QueueGridTable.Columns = [
                ("#", new GridLength(60, GridUnitType.Pixel)),
                ("Title", new GridLength(1, GridUnitType.Star)),
                ("Artist", new GridLength(1, GridUnitType.Star)),
                ("Album", new GridLength(1, GridUnitType.Star))
            ];
        }

        private void Queue_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            ChangeColorIndex();
        }

        private void ChangeColorIndex()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                RevertPreviousSongColor();
                if (Queue.CurrentItemIndex < PlayerPlaylist.Instance.Songs.Count)
                {
                    var rect = QueueGridTable.GetRectangle((int)Queue.CurrentItemIndex);
                    QueueGridTable.RectangleColors[rect] = true;
                    rect.Fill = QueueGridTable.Colors[true].Fill;
                    PreviousSongRectangle = rect;
                }
            });
        }

        private void RevertPreviousSongColor()
        {
            if (PreviousSongRectangle != null)
            {
                QueueGridTable.RectangleColors[PreviousSongRectangle] = false;
                PreviousSongRectangle.Fill = QueueGridTable.Colors[false].Fill;
                PreviousSongRectangle = null;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ListSongs();
        }

        private void ListSongs()
        {
            QueueGridTable.Clear();
            Songs.Clear();
            int index = 1;
            foreach (var song in PlayerPlaylist.Instance.Songs)
            {
                TimeSpan duration = TimeSpan.FromSeconds(song.Duration);
                Dictionary<string, string?> dic = new()
                {
                    ["#"] = string.Format("{0:D" + PlayerPlaylist.Instance.Songs.Count.ToString().Length + "}", index++),
                    ["Title"] = song.Title,
                    ["Artist"] = song.Artist,
                    ["Album"] = song.Album,
                };
                QueueGridTable.AddRow(dic);
                Songs.Add(song);
            }
            QueueGridTable.ShowContent();
            ChangeColorIndex();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerPlaylist.Instance.ClearSongs();
            Songs.Clear();
            QueueGridTable.Clear();
            QueueGridTable.ShowContent();
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException(); // TODO
        }

        private CommandBarFlyout QueueGridTable_RowRightTapped(object sender, RowEvent e)
        {
            return QueueSongCommandBarFlyout.Create(QueueGridTable, (uint)e.Index, Queue);
        }

        private void QueueGridTable_RowTapped(object sender, RowEvent e)
        {
            app.ForcefulSongChange = true;
            Queue.MoveTo((uint)e.Index);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Queue.CurrentItemChanged -= Queue_CurrentItemChanged;
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            switch (app.RepeatMode)
            {
                case RepeatMode.OFF:
                    app.RepeatMode = RepeatMode.ALL;
                    break;
                case RepeatMode.ALL:
                    app.RepeatMode = RepeatMode.ONE;
                    break;
                case RepeatMode.ONE:
                    app.RepeatMode = RepeatMode.OFF;
                    break;
            }
            RepeatButton.IsChecked = GetRepeatChecked();
            ((FontIcon)RepeatButton.Icon).Glyph = GetRepeatGlyph();
        }

        private string GetRepeatGlyph()
        {
            return app.RepeatMode switch
            {
                RepeatMode.ALL => "\uE8EE",
                RepeatMode.ONE => "\uE8ED",
                _ => "\uF5E7",
            };
        }

        private bool GetRepeatChecked()
        {
            return app.RepeatMode switch
            {
                RepeatMode.ALL => true,
                RepeatMode.ONE => true,
                _ => false,
            };
        }
    }
}
