﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace WinSonic.Pages.Converter
{
    internal class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether to invert the conversion.
        /// If true, true will convert to Collapsed and false will convert to Visible.
        /// </summary>
        public bool Invert { get; set; }

        /// <summary>
        /// Converts a boolean value to a Visibility enum value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">Optional parameter. If "invert" or "true", inverts the conversion.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>Visibility.Visible if the value is true (or false if inverted), Visibility.Collapsed otherwise.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool boolValue = value is bool val && val;

            // Check if conversion should be inverted based on parameter or property
            bool invert = Invert;
            if (parameter != null)
            {
                string? param = parameter.ToString()?.ToLowerInvariant();
                if (param == "invert" || param == "true")
                {
                    invert = true;
                }
            }

            if (invert)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility enum value back to a boolean value.
        /// </summary>
        /// <param name="value">The Visibility enum value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">Optional parameter. If "invert" or "true", inverts the conversion.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>true if the value is Visibility.Visible (or Collapsed if inverted), false otherwise.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // Handle the case where value is null
            if (value == null)
                return false;

            Visibility visibility = (Visibility)value;

            // Check if conversion should be inverted based on parameter or property
            bool invert = Invert;
            if (parameter != null)
            {
                string? param = parameter.ToString()?.ToLowerInvariant();
                if (param == "invert" || param == "true")
                {
                    invert = true;
                }
            }

            if (invert)
            {
                return visibility != Visibility.Visible;
            }

            return visibility == Visibility.Visible;
        }
    }
}
