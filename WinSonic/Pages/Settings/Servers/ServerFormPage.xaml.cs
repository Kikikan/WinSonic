using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WinSonic.Model;
using WinSonic.Model.Api;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Settings.Servers;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerFormPage : Page, INotifyPropertyChanged
{
    private bool _isServerEditable = true;

    public bool IsServerEditable
    {
        get => _isServerEditable;
        private set
        {
            if (_isServerEditable != value)
            {
                _isServerEditable = value;
                OnPropertyChanged(nameof(IsServerEditable));
            }
        }
    }

    private bool _connectionSuccessful = false;

    public bool IsConnectionSuccessful
    {
        get => _connectionSuccessful;
        private set
        {
            if (_connectionSuccessful != value)
            {
                _connectionSuccessful = value;
                OnPropertyChanged(nameof(IsConnectionSuccessful));
            }
        }
    }

    private Server? oldServer;
    private Server? server;

    public ServerFormPage()
    {
        InitializeComponent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is Server server)
        {
            oldServer = server;
            this.server = server;
            NameTextBox.Text = server.Name;
            URLTextBox.Text = server.Address;
            UsernameTextBox.Text = server.Username;
        }
    }

    private async void Connect_Click(object sender, RoutedEventArgs e)
    {
        IsServerEditable = false;
        ConnectionTestRing.IsActive = true;
        TestConnectionButton.IsEnabled = false;
        TestConnectionText.Visibility = Visibility.Collapsed;
        IsConnectionSuccessful = false;
        string messageText;
        try
        {
            if (oldServer == null || !string.IsNullOrEmpty(PasswordTextBox.Password))
            {
                server = new Server(NameTextBox.Text, URLTextBox.Text, UsernameTextBox.Text, PasswordTextBox.Password);
            }
            else
            {
                server = new Server(NameTextBox.Text, URLTextBox.Text, UsernameTextBox.Text, oldServer.PasswordHash, oldServer.Salt);
            }
            var rs = await SubsonicApiHelper.Ping(server);

            messageText = rs.Status == ResponseStatus.Ok
                ? "The connection was successful."
                : $"Subsonic error: {rs.Error.Message}";
            IsConnectionSuccessful = rs.Status == ResponseStatus.Ok;
        }
        catch (TaskCanceledException)
        {
            messageText = $"No response was received.";
        }
        catch (Exception ex)
        {
            messageText = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            ConnectionTestRing.IsActive = false;
            TestConnectionText.Visibility = Visibility.Visible;

            if (!IsConnectionSuccessful)
            {
                IsServerEditable = true;
            }
        }

        TextBlock flyoutText = new()
        {
            Text = messageText,
            Padding = new Thickness(10)
        };

        Flyout flyout = new()
        {
            Content = flyoutText
        };

        flyout.ShowAt((FrameworkElement)sender);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (server is not null)
        {
            var roamingSettings = ((App)Application.Current).RoamingSettings;
            bool success;

            if (oldServer == null)
            {
                success = roamingSettings.ServerSettings.AddServer(server);
            }
            else
            {
                success = roamingSettings.ServerSettings.ReplaceServer(oldServer, server);
            }
            string message;
            if (success)
            {
                roamingSettings.SaveSetting(roamingSettings.ServerSettings);
                if (oldServer == null)
                {
                    message = "Server was successfully saved.";
                }
                else
                {
                    message = "Server was successfully edited.";
                    DispatcherQueue.TryEnqueue(() => Frame.GoBack());
                }
            }
            else
            {
                message = "Server is already saved.";
            }
            NameTextBox.Text = "";
            URLTextBox.Text = "";
            UsernameTextBox.Text = "";
            PasswordTextBox.Password = "";
            IsServerEditable = true;
            IsConnectionSuccessful = false;

            TextBlock flyoutText = new()
            {
                Text = message,
                Padding = new Thickness(10)
            };


            Flyout flyout = new()
            {
                Content = flyoutText
            };

            flyout.ShowAt((FrameworkElement)sender);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsServerEditable = true;
        IsConnectionSuccessful = false;
        if (oldServer != null)
        {
            Frame.GoBack();
        }
    }
}
