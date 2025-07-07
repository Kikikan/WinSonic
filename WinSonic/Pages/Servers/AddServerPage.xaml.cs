using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WinSonic.Model;
using WinSonic.Model.Api;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AddServerPage : Page, INotifyPropertyChanged
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

    private Server? server;

    public AddServerPage()
    {
        InitializeComponent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
            server = new Server(NameTextBox.Text, URL.Text, Username.Text, Password.Password);
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
            bool added = roamingSettings.ServerSettings.AddServer(server);
            string message;
            if (added)
            {
                roamingSettings.SaveSetting(roamingSettings.ServerSettings);
                message = "Server was successfully saved.";
            }
            else
            {
                message = "Server is already saved.";
            }
            NameTextBox.Text = "";
            URL.Text = "";
            Username.Text = "";
            Password.Password = "";
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
    }
}
