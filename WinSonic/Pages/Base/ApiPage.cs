using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSonic.Controls;
using WinSonic.Model.Api;

namespace WinSonic.Pages.Base
{
    public class ApiPage : Page
    {
        protected async Task<T?> TryApiCall<T>(Func<Task<T>> apiCall)
        {
            try
            {
                return await apiCall();
            }
            catch (ApiException ex)
            {
                ExceptionContentDialog.Show(ex, this);
                return default;
            }
        }
    }
}
