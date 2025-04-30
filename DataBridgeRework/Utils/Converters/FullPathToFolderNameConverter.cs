using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace DataBridgeRework.Utils.Converters;

public class FullPathToFolderNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string fullPath)
        {
            // Удаляем завершающий разделитель, если есть
            fullPath = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Если путь пустой после удаления разделителя, значит, это корневая директория
            if (string.IsNullOrEmpty(fullPath))
                return "/"; // Вернем корень вместо пустой строки

            return Path.GetFileName(fullPath);
        }

        return value;
        ;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}