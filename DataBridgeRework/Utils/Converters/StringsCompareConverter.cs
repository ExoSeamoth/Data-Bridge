using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace DataBridgeRework.Utils.Converters;

public sealed class StringsCompareConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string aValue && parameter is string bValue)
            return aValue == bValue;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is true && parameter is string stringValue)
            return stringValue;

        return BindingOperations.DoNothing;
    }
}