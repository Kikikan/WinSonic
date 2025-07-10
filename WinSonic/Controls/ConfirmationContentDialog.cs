using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinSonic.Controls
{
    public class ConfirmationContentDialog
    {
        public static ContentDialog CreateDialog(XamlRoot xamlRoot)
        {
            ContentDialog dialog = new()
            {
                Title = "Are you sure?",
                Content = "This action cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = xamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                DefaultButton = ContentDialogButton.Close
            };
            return dialog;
        }
    }
}
