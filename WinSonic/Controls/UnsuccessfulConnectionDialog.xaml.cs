using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WinSonic.Model;
using WinSonic.Model.Settings;

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

        private static (ContentDialog, UnsuccessfulConnectionDialog) CreateDialog(XamlRoot root, List<Server> servers)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = root,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Unsuccessful Connection",
                PrimaryButtonText = "Retry for selected",
                CloseButtonText = "Close",
                IsSecondaryButtonEnabled = false,
                DefaultButton = ContentDialogButton.Primary
            };
            var content = new UnsuccessfulConnectionDialog();
            foreach (var server in servers)
            {
                content.Servers.Add(server);
            }
            dialog.Content = content;
            return (dialog, content);
        }

        public static async Task ShowDialog(XamlRoot root, List<Server> servers)
        {
            if (servers != null && servers.Count > 0)
            {
                var dialog = CreateDialog(root, servers);

                var result = await dialog.Item1.ShowAsync();
                if (result == ContentDialogResult.Primary && dialog.Item2.ServerListView != null)
                {
                    List<Server> attemptList = [];
                    foreach (var obj in dialog.Item2.ServerListView.SelectedItems)
                    {
                        if (obj is Server server)
                        {
                            attemptList.Add(server);
                        }
                    }
                    List<Server> attemptResult = await ServerSettingGroup.TryPing(attemptList);
                    await ShowDialog(root, attemptResult);
                }
            }
        }
    }
}
