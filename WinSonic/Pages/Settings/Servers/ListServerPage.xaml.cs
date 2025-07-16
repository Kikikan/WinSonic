using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WinSonic.Controls;
using WinSonic.Model;
using WinSonic.Model.Settings;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Settings.Servers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListServerPage : Page
    {
        private readonly RoamingSettings roamingSettings = ((App)Application.Current).RoamingSettings;
        private readonly ObservableCollection<Server> servers = [];
        private bool initialized = false;
        private bool pinging = false;
        public ListServerPage()
        {
            InitializeComponent();
            roamingSettings.ServerSettings.Servers.ForEach(servers.Add);
        }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (initialized)
            {
                if (sender is ToggleSwitch toggle && toggle.Tag is Server server && toggle.IsOn)
                {
                    _ = Task.Delay(500).ContinueWith(t => SetIsLoading(true));
                    toggle.IsEnabled = false;
                    pinging = true;
                    var successful = (await ServerSettingGroup.TryPing([server])).Count == 0;
                    if (!successful)
                    {
                        toggle.IsOn = false;
                        await UnsuccessfulConnectionDialog.ShowDialog(toggle.XamlRoot, [server]);
                        toggle.IsOn = server.Enabled;
                    }
                    pinging = false;
                    toggle.IsEnabled = true;
                    SetIsLoading(false);
                }
                DispatcherQueue.TryEnqueue(() => roamingSettings.SaveSetting(roamingSettings.ServerSettings));
            }
        }

        private void SetIsLoading(bool value)
        {
            if (Application.Current is App app && app.Window != null && (!value || pinging))
            {
                DispatcherQueue.TryEnqueue(() => app.Window.IsLoading = value);
            }
        }

        private void SettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is Server server)
            {
                Frame.Navigate(typeof(ServerFormPage), server, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            initialized = true;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is Server server)
            {
                ContentDialog dialog = DeleteConfirmationContentDialog.CreateDialog(XamlRoot);
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    roamingSettings.ServerSettings.RemoveServer(server);
                    servers.Remove(server);
                    roamingSettings.SaveSetting(roamingSettings.ServerSettings);
                }
            }
        }
    }
}
