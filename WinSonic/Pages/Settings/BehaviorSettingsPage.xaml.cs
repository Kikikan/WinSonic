using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinSonic.Model.Settings;
using WinSonic.Model.Util;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BehaviorSettingsPage : Page
    {
        private readonly RoamingSettings settings = ((App)Application.Current).RoamingSettings;
        public BehaviorSettingsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AlbumGridTableComboBox.ItemsSource = EnumExtensions.GetDisplayItems<BehaviorSettings.GridTableDoubleClickBehavior>();
            AlbumGridTableComboBox.DisplayMemberPath = "DisplayName";
            AlbumGridTableComboBox.SelectedValuePath = "Value";
            AlbumGridTableComboBox.SelectedIndex = (int)settings.BehaviorSettings.AlbumDoubleClickBehavior;

            PlaylistGridTableComboBox.ItemsSource = EnumExtensions.GetDisplayItems<BehaviorSettings.GridTableDoubleClickBehavior>();
            PlaylistGridTableComboBox.DisplayMemberPath = "DisplayName";
            PlaylistGridTableComboBox.SelectedValuePath = "Value";
            PlaylistGridTableComboBox.SelectedIndex = (int)settings.BehaviorSettings.PlaylistDoubleClickBehavior;
        }

        private void AlbumGridTableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.BehaviorSettings.AlbumDoubleClickBehavior = (BehaviorSettings.GridTableDoubleClickBehavior) AlbumGridTableComboBox.SelectedIndex;
            settings.SaveSetting(settings.BehaviorSettings);
        }

        private void PlaylistGridTableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.BehaviorSettings.PlaylistDoubleClickBehavior = (BehaviorSettings.GridTableDoubleClickBehavior)PlaylistGridTableComboBox.SelectedIndex;
            settings.SaveSetting(settings.BehaviorSettings);
        }
    }
}
