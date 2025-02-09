using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Data_Bridge.Converters;
using Data_Bridge.Entries;
using Data_Bridge.Views;
using Renci.SshNet;

namespace Data_Bridge.ViewModels;

public partial class ConnectionsWindowViewModel : ObservableObject
{
    [ObservableProperty] private SftpConnectionEntry _selectedConnection = new();
    
    public ObservableCollection<SftpConnectionEntry> SavedConnections { get; set; } = new() {  };
    
    public async Task OpenFileDialog(ConnectionsWindow window)
    {
        var filePicker = new FilePickerOpenOptions()
        {
            Title = "Выберите ssh-ключ",
            SuggestedFileName = "id_rsa",
            FileTypeFilter =
            [
                new("ssh-keys")
                {
                    Patterns = ["*.pub", "*"],
                    MimeTypes = ["application/octet-stream", "text/plain"]
                },
                FilePickerFileTypes.All,
            ],
            AllowMultiple = false,
        };
        var result = await window.StorageProvider.OpenFilePickerAsync(filePicker);

        if (result.Count > 0)
        {
            var selectedFile = result[0];
            SelectedConnection.SshKeyPath = selectedFile.Path.AbsolutePath;
        }
    }

    public SftpClient? TryBuildConnect()
    {
        AuthenticationMethod authMethod = new NoneAuthenticationMethod(SelectedConnection.UserName);
        switch (SelectedConnection.SecurityType)
        {
            case SecurityType.Password:
                authMethod = new PasswordAuthenticationMethod(SelectedConnection.UserName,
                    SelectedConnection.Password);
                break;
            case SecurityType.SshKey:
                PrivateKeyFile privateKey;
                if (string.IsNullOrEmpty(SelectedConnection.PassPhrase))
                    privateKey = new(SelectedConnection.SshKeyPath);
                else
                    privateKey = new(SelectedConnection.SshKeyPath, SelectedConnection.PassPhrase);

                authMethod = new PrivateKeyAuthenticationMethod(SelectedConnection.UserName, privateKey);
                break;
        }

        ConnectionInfo connectionInfo = new(
            SelectedConnection.HostName, SelectedConnection.Port,
            SelectedConnection.UserName, authMethod);
        
        return new SftpClient(connectionInfo);
    }

    private bool IsDataValid()
    {
        bool isHostValid = !string.IsNullOrEmpty(SelectedConnection.HostName);
        bool isUserValid = !string.IsNullOrEmpty(SelectedConnection.UserName);

        bool isSecurityValid = false;
        switch (SelectedConnection.SecurityType)
        {
            case SecurityType.Password:
                isSecurityValid = !string.IsNullOrEmpty(SelectedConnection.Password);
                break;
            case SecurityType.SshKey:
                isSecurityValid = !string.IsNullOrEmpty(SelectedConnection.SshKeyPath);
                break;
        }

        return isHostValid && isUserValid && isSecurityValid;
    }
}