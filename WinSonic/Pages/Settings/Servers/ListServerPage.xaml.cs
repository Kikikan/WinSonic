using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WinSonic.Model;
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
        public ListServerPage()
        {
            InitializeComponent();
            roamingSettings.ServerSettings.Servers.ForEach(servers.Add);
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (initialized)
            {
                DispatcherQueue.TryEnqueue(() => roamingSettings.SaveSetting(roamingSettings.ServerSettings));
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
    }
}
