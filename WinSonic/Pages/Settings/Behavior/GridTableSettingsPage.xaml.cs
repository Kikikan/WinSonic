using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinSonic.Model.Settings;
using WinSonic.Model.Util;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Settings.Behavior
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GridTableSettingsPage : Page
    {
        private readonly RoamingSettings settings = ((App)Application.Current).RoamingSettings;
        public GridTableSettingsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AlbumGridTableComboBox.ItemsSource = EnumExtensions.GetDisplayItems<BehaviorSettingGroup.GridTableDoubleClickBehavior>();
            AlbumGridTableComboBox.DisplayMemberPath = "DisplayName";
            AlbumGridTableComboBox.SelectedValuePath = "Value";
            AlbumGridTableComboBox.SelectedIndex = (int)settings.BehaviorSettings.AlbumDoubleClickBehavior;

            PlaylistGridTableComboBox.ItemsSource = EnumExtensions.GetDisplayItems<BehaviorSettingGroup.GridTableDoubleClickBehavior>();
            PlaylistGridTableComboBox.DisplayMemberPath = "DisplayName";
            PlaylistGridTableComboBox.SelectedValuePath = "Value";
            PlaylistGridTableComboBox.SelectedIndex = (int)settings.BehaviorSettings.PlaylistDoubleClickBehavior;
        }

        private void AlbumGridTableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.BehaviorSettings.AlbumDoubleClickBehavior = (BehaviorSettingGroup.GridTableDoubleClickBehavior)AlbumGridTableComboBox.SelectedIndex;
            settings.SaveSetting(settings.BehaviorSettings);
        }

        private void PlaylistGridTableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.BehaviorSettings.PlaylistDoubleClickBehavior = (BehaviorSettingGroup.GridTableDoubleClickBehavior)PlaylistGridTableComboBox.SelectedIndex;
            settings.SaveSetting(settings.BehaviorSettings);
        }
    }
}
