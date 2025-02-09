using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Renci.SshNet.Sftp;

namespace Data_Bridge.Converters;

public class ISftpItemToSizeConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ISftpFile b && b.IsRegularFile)
            return $"{b.Attributes.Size / 1000} KB";
        
        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}