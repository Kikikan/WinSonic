using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSonic.Model.Api;

namespace WinSonic.Controls
{
    public class ExceptionContentDialog
    {
        public static async void Show(ApiException ex, Page page)
        {
            var dialog = new ContentDialog
            {
                Title = $"Could not connect to {ex.Server.Name}",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = page.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
