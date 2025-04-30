using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBridgeRework.Utils.Models;

namespace DataBridgeRework.ViewModels;

public partial class ConnectionWindowViewModel : ObservableValidator
{
    [ObservableProperty] [Required(ErrorMessage = "Введите адрес сервера.")]
    private string _hostName = string.Empty;

    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] [Range(1, 65535)] [Required(ErrorMessage = "Введите порт.")]
    private ushort _port = 22;

    private ObservableCollection<ServerConnectionData> _savedConnections = [];

    [ObservableProperty] private SecurityType _securityType = SecurityType.Password;

    [ObservableProperty] [Required(ErrorMessage = "Укажите путь до ключа.")]
    private string _sshKeyPath = string.Empty;

    [ObservableProperty] private string _sshKeyPhrase = string.Empty;

    [ObservableProperty] [Required(ErrorMessage = "Введите имя пользователя.")]
    private string _userName = string.Empty;

    [RelayCommand]
    private void ConnectToServer(Window window)
    {
        ValidateFields();
        if (HasErrors) return;
        Console.WriteLine("Trying to connect");
    }

    [RelayCommand]
    private async Task OpenFileDialog(Window window)
    {
        var filePicker = new FilePickerOpenOptions
        {
            Title = "Выберите ssh-ключ",
            SuggestedFileName = "id_rsa",
            FileTypeFilter =
            [
                new FilePickerFileType("ssh-keys")
                {
                    Patterns = ["*.pub", "*"],
                    MimeTypes = ["application/octet-stream", "text/plain"]
                },
                FilePickerFileTypes.All
            ],
            AllowMultiple = false
        };
        var result = await window.StorageProvider.OpenFilePickerAsync(filePicker);

        if (result.Count > 0)
        {
            var selectedFile = result[0];
            SshKeyPath = selectedFile.Path.AbsolutePath;
        }
    }

    [RelayCommand]
    private void SaveConnection()
    {
        ValidateFields();
        if (HasErrors) return;
        Console.WriteLine("Trying to save the connection");
    }

    private void ValidateFields()
    {
        ValidateProperty(Port, nameof(Port));
        ValidateProperty(UserName, nameof(UserName));
        ValidateProperty(HostName, nameof(HostName));
        switch (SecurityType)
        {
            case SecurityType.Password:
                break;
            case SecurityType.SshKey:
                ValidateProperty(SshKeyPath, nameof(SshKeyPath));
                break;
        }
    }
}