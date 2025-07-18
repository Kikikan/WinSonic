using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WinSonic.Pages.Settings.Behavior;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BehaviorSettingsPage : Page
    {
        public BehaviorSettingsPage()
        {
            InitializeComponent();
        }

        private void GridTableButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GridTableSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
