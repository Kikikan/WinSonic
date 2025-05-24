using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using WinSonic.Model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UnsuccessfulConnectionDialog : Page
    {
        public ObservableCollection<Server> Servers { get; private set; } = [];
        public ListView? ServerListView { get; private set; }
        public UnsuccessfulConnectionDialog()
        {
            InitializeComponent();
        }

        private void ServerList_Loaded(object sender, RoutedEventArgs e)
        {
            ServerListView = ServerList;
        }
    }
}
