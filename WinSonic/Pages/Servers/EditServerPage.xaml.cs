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
        private readonly ServerFile serverFile = ((App)Application.Current).ServerFile;
        internal List<Server> Servers { get; }
        private bool initialized = false;
        public EditServerPage()
        {
            InitializeComponent();
            Servers = serverFile.Servers;
            OnPropertyChanged(nameof(Servers));
        }

        private void ServerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initialized)
            {
                foreach (var server in Servers)
                {
                    server.Enabled = ServerListView.SelectedItems.Contains(server);
                }
                serverFile.Save();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            SelectedServer = Servers[ServerListView.SelectedIndex];
            OnPropertyChanged(nameof(SelectedServer));
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var item = element.DataContext;
                if (item is Server server)
                {
                    Servers.Remove(server);
                    serverFile.Save();
                    OnPropertyChanged(nameof(Servers));
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
