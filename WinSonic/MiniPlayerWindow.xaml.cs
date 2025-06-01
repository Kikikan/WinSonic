using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Media.Playback;
using WinSonic.Model.Api;
using WinSonic.Model.Player;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MiniPlayerWindow : Window, INotifyPropertyChanged
    {
        private nint _hwnd;
        private WindowProc? _newWndProc;
        private nint _oldWndProc;
        private readonly MediaPlaybackList _mediaPlaybackList;
        private Song? _currentSong;
        public Song? CurrentSong { get { return _currentSong; } set { _currentSong = value; DispatcherQueue.TryEnqueue(() => OnPropertyChanged(nameof(CurrentSong))); } }

        public MiniPlayerWindow()
        {
            this.InitializeComponent();
            if (Application.Current is App app)
            {
                _mediaPlaybackList = app.MediaPlaybackList;
                _mediaPlaybackList.CurrentItemChanged += _mediaPlaybackList_CurrentItemChanged;
                CurrentSong = PlayerPlaylist.Instance.Songs[(int)_mediaPlaybackList.CurrentItemIndex];
            }
            else
            {
                throw new Exception("Application is not an App.");
            }

            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(null);
            AppWindow.SetIcon(null);
            AppWindow.IsShownInSwitchers = false;
            RootGrid.DataContext = this; // Set DataContext on the root Grid

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            _hwnd = hwnd;
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
            var presenter = appWindow.Presenter as OverlappedPresenter;
            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.SetBorderAndTitleBar(false, false);
                appWindow.SetPresenter(overlappedPresenter);
            }
            appWindow.IsShownInSwitchers = false;

            // Remove window border and caption to make it non-movable
            MakeWindowNonMovable(_hwnd);

            // Disable the close button
            DisableCloseButton(_hwnd);

            // Position at bottom-right above the taskbar
            PositionBottomRight(_hwnd);

            // Try to make it truly borderless
            MakeWindowTrulyBorderless(_hwnd);

            HookWindowMessages();

            // Focus the window
            this.Activate();
            SetForegroundWindow(_hwnd);

            MiniMediaPlayer.SetMediaPlayer(MainWindow.Instance?.SharedMediaPlayer);
        }

        private void _mediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (sender.CurrentItemIndex < PlayerPlaylist.Instance.Songs.Count)
            {
                CurrentSong = PlayerPlaylist.Instance.Songs[(int)sender.CurrentItemIndex];
            }
        }

        private void HookWindowMessages()
        {
            _newWndProc = new WindowProc(WndProc);
            _oldWndProc = SetWindowLongPtr(_hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));
        }

        private nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
        {
            const uint WM_ACTIVATE = 0x0006;
            const uint WM_NCHITTEST = 0x0084;
            const int HTNOWHERE = 0;

            if (msg == WM_ACTIVATE)
            {
                int LOWORD = (int)(wParam.ToInt64() & 0xFFFF);
                if (LOWORD == 0) // WA_INACTIVE
                {
                    Close();
                }
            }
            else if (msg == WM_NCHITTEST)
            {
                // Prevent window from being moved by dragging
                return HTNOWHERE;
            }

            return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
        }

        // Helper to remove window border and caption
        private void MakeWindowNonMovable(nint hwnd)
        {
            const int GWL_STYLE = -16;
            const int WS_CAPTION = 0x00C00000;
            const int WS_THICKFRAME = 0x00040000;
            const int WS_SYSMENU = 0x00080000;
            const int WS_MINIMIZEBOX = 0x00020000;

            int style = GetWindowLong(hwnd, GWL_STYLE);
            style &= ~WS_CAPTION;
            style &= ~WS_THICKFRAME;
            style &= ~WS_SYSMENU;
            style &= ~WS_MINIMIZEBOX;
            SetWindowLong(hwnd, GWL_STYLE, style);
        }

        private void MakeWindowTrulyBorderless(nint hwnd)
        {
            const int GWL_STYLE = -16;
            const int WS_POPUP = unchecked((int)0x80000000);
            // Remove all other styles, set only WS_POPUP
            SetWindowLong(hwnd, GWL_STYLE, WS_POPUP);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(nint hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

        // Helper to disable the close button
        private void DisableCloseButton(IntPtr hwnd)
        {
            var hMenu = GetSystemMenu(hwnd, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        private const uint SC_CLOSE = 0xF060;
        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_GRAYED = 0x00000001;

        // Helper to position window at bottom-right above the taskbar
        private void PositionBottomRight(nint hwnd)
        {
            var displayInfo = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(
                Win32Interop.GetWindowIdFromWindow(hwnd),
                Microsoft.UI.Windowing.DisplayAreaFallback.Primary
            );
            int windowWidth = 300;
            int windowHeight = 180;
            int x = displayInfo.WorkArea.X + displayInfo.WorkArea.Width - windowWidth;
            int y = displayInfo.WorkArea.Y + displayInfo.WorkArea.Height - windowHeight;

            SetWindowPos(hwnd, IntPtr.Zero, x, y, windowWidth, windowHeight, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        #region Win32 Interop
        private delegate nint WindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

        [DllImport("user32.dll")]
        private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

        [DllImport("user32.dll")]
        private static extern nint CallWindowProc(nint lpPrevWndFunc, nint hWnd, uint Msg, nint wParam, nint lParam);

        private const int GWLP_WNDPROC = -4;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
