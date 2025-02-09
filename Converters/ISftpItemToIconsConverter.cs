using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Controls;
using Renci.SshNet.Sftp;

namespace Data_Bridge.Converters;

public class ISftpItemToIconsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ISftpFile item)
        {
            ImageIconSource icon = new ImageIconSource { Source = new Bitmap(AssetLoader.Open(new Uri("avares://Data Bridge/Assets/Icons/file.png"))) };
            if (item.IsDirectory)
                icon.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Data Bridge/Assets/Icons/folder.png")));
            if (item.IsSymbolicLink)
                icon.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Data Bridge/Assets/Icons/symlink-file.png")));

            return icon;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}