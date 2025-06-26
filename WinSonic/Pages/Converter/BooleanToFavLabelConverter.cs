using Microsoft.UI.Xaml.Data;
using System;

namespace WinSonic.Pages.Converter
{
    internal partial class BooleanToFavLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool boolValue = value is bool val && val;
            return boolValue ? "Unfavourite" : "Favourite";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return false;

            string visibility = (string)value;
            return visibility == "Unfavourite";
        }
    }
}
