using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using WinSonic.Model;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditServerPage : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        internal bool IsEditing { get; set; } = false;
        internal Server? SelectedServer { get; set; }
        private readonly RoamingSettings serverFile = ((App)Application.Current).RoamingSettings;
        internal readonly List<Server> servers = [];
        private bool initialized = false;
        public EditServerPage()
        {
            InitializeComponent();
            servers.AddRange(serverFile.ActiveServers);
            OnPropertyChanged(nameof(servers));
        }

        private void ServerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initialized)
            {
                foreach (var server in servers)
                {
                    server.Enabled = ServerListView.SelectedItems.Contains(server);
                }
                serverFile.SaveServers();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            SelectedServer = servers[ServerListView.SelectedIndex];
            OnPropertyChanged(nameof(SelectedServer));
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var item = element.DataContext;
                if (item is Server server)
                {
                    servers.Remove(server);
                    serverFile.SaveServers();
                    OnPropertyChanged(nameof(servers));
                }
            }
        }

        private void ServerListView_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var element in ServerListView.Items)
            {
                if (element is Server server && server.Enabled)
                {
                    ServerListView.SelectedItems.Add(server);
                }
            }
            initialized = true;
        }
    }
}
