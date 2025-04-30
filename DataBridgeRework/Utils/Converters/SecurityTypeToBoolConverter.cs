using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using DataBridgeRework.Utils.Models;

namespace DataBridgeRework.Utils.Converters;

public sealed class SecurityTypeToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // parameter - это значение SecurityType, передаваемое из XAML.
        if (value is SecurityType securityType && parameter is string enumValue)
            return securityType.ToString() == enumValue;

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Если RadioButton установлен (IsChecked == true), вернем enum.
        if (value is bool isChecked and true && parameter is string enumValue)
            return Enum.Parse(typeof(SecurityType), enumValue);

        return BindingOperations.DoNothing;
    }
}