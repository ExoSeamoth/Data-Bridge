using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Data_Bridge.ViewModels;
using FluentAvalonia.UI.Controls;

namespace Data_Bridge.Views;

public partial class ConnectionsWindow : Window
{
    public ConnectionsWindow()
    {
        DataContext = new ConnectionsWindowViewModel();
        InitializeComponent();
    }

    private void OpenFileDialog_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ConnectionsWindowViewModel vm)
            _ = vm.OpenFileDialog(this);
    }

    private void SaveConnection_OnClick(object? sender, RoutedEventArgs e)
    {
        //TODO: сделать реализацию сохранения подключения
    }

    private async void ConnectToServer_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is ConnectionsWindowViewModel vm)
            {
                var result = vm.TryBuildConnect();
                Close(result);
            }
        }
        catch (ArgumentException exception)
        {
            var dialog = new TaskDialog()
            {
                Title = "Ошибка",
                Header = "Некорректные данные",
                Content = "Пожалуйста, проверьте введенные данные и попробуйте снова.\n\n" +
                          "Подробная информация:\n" +
                          $"{exception.Message}",
                XamlRoot = this,
                Buttons = new List<TaskDialogButton> { TaskDialogButton.OKButton },
                IconSource = new BitmapIconSource{ UriSource = new("avares://Data Bridge/Assets/Icons/error.png")},
                MaxWidth = 500
            };

            await dialog.ShowAsync();
        }
        catch (Exception exception)
        {
            var dialog = new TaskDialog()
            {
                Title = "Ошибка",
                Header = "Неизвестная ошибка",
                Content = exception.Message,
                XamlRoot = this,
                Buttons = new List<TaskDialogButton> { TaskDialogButton.OKButton },
                IconSource = new BitmapIconSource{ UriSource = new("avares://Data Bridge/Assets/Icons/error.png")},
            };

            await dialog.ShowAsync();
        }
    }
}

