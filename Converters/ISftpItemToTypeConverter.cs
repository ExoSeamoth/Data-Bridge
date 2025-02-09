using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Renci.SshNet.Sftp;


namespace Data_Bridge.Converters;

public class ISftpItemToTypeConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ISftpFile item)
        {
            if (item.IsDirectory)
                return "Папка с файлами";
            if (item.IsSymbolicLink)
                return "Символическая ссылка";

            string extension = Path.GetExtension(item.FullName).ToUpper();

            if (extension != "")
                return $"Файл \"{extension.TrimStart('.')}\"";
            
            return "Файл";
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}