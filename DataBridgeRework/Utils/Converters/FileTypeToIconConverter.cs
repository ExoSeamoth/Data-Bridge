using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DataBridgeRework.Utils.Models;
using FluentAvalonia.UI.Controls;

namespace DataBridgeRework.Utils.Converters;

public sealed class FileTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not FileType fileType) return null;
        
        Bitmap image = fileType switch
        {
            FileType.Directory => new(AssetLoader.Open(new Uri("avares://DataBridgeRework/Assets/Icons/folder.png"))),
            FileType.File => new(AssetLoader.Open(new Uri("avares://DataBridgeRework/Assets/Icons/file.png"))),
            FileType.SymbolicLink => new(AssetLoader.Open(new Uri("avares://DataBridgeRework/Assets/Icons/symlink-file.png"))),
            _ => new(AssetLoader.Open(new Uri("avares://DataBridgeRework/Assets/Icons/file.png")))
        };

        return new ImageIconSource { Source = image };

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}