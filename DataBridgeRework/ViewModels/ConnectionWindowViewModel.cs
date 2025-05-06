using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBridgeRework.Utils.Converters;
using DataBridgeRework.Utils.Models;

namespace DataBridgeRework.ViewModels;

public partial class ConnectionWindowViewModel : ObservableObject
{
    private const string SAVE_CONNECTIONS_PATH = "connections.json";

    [ObservableProperty] private int _selectedConnectionIndex = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    [NotifyPropertyChangedFor(nameof(IsNewConnection))]
    private ServerConnectionData _selectedServerConnection = new();

    public ConnectionWindowViewModel()
    {
        SavedConnections = new ObservableCollection<ServerConnectionData>(GetConnectionsFromJson());
    }

    public ObservableCollection<ServerConnectionData> SavedConnections { get; init; }

    public bool IsNewConnection => SelectedConnectionIndex == -1;
    public string SaveButtonText => IsNewConnection ? "Сохранить" : "Копировать";

    partial void OnSelectedConnectionIndexChanged(int value)
    {
        if (value >= 0 && value < SavedConnections.Count)
            SelectedServerConnection = SavedConnections[value];
    }

    [RelayCommand]
    private void ConnectToServer(Window window)
    {
        SelectedServerConnection.ValidateFields();
        if (SelectedServerConnection.HasErrors) return;

        window.Close(SelectedServerConnection);
    }

    [RelayCommand]
    private void CreateNewConnection()
    {
        SelectedConnectionIndex = -1;
        SelectedServerConnection = new ServerConnectionData();
    }

    [RelayCommand]
    private void DeleteConnection(ServerConnectionData connection)
    {
        SavedConnections.Remove(connection);
        CreateNewConnectionCommand.Execute(null);
    }

    [RelayCommand]
    private void AddConnection()
    {
        SelectedServerConnection.ValidateFields();
        if (SelectedServerConnection.HasErrors) return;

        ServerConnectionData connection = new()
        {
            HostName = SelectedServerConnection.HostName,
            Port = SelectedServerConnection.Port,
            UserName = SelectedServerConnection.UserName,
            Password = SelectedServerConnection.Password,
            SshKeyPath = SelectedServerConnection.SshKeyPath,
            SshKeyPhrase = SelectedServerConnection.SshKeyPhrase,
            SecurityType = SelectedServerConnection.SecurityType
        };

        SavedConnections.Add(connection);

        SelectedConnectionIndex = SavedConnections.Count - 1;
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
            SelectedServerConnection.SshKeyPath = selectedFile.Path.AbsolutePath;
        }
    }

    public IEnumerable<ServerConnectionData> GetConnectionsFromJson()
    {
        if (File.Exists(SAVE_CONNECTIONS_PATH))
        {
            var jsonData = File.ReadAllText(SAVE_CONNECTIONS_PATH);
            return JsonSerializer.Deserialize(jsonData, AppJsonSerializerContext.Default.IEnumerableServerConnectionData);
        }

        File.WriteAllText(SAVE_CONNECTIONS_PATH, "[\n]");

        return new List<ServerConnectionData>();
    }

    public void SaveConnectionsToJson()
    {
        var jsonData = JsonSerializer.Serialize(SavedConnections, AppJsonSerializerContext.Default.IEnumerableServerConnectionData);
        File.WriteAllText(SAVE_CONNECTIONS_PATH, jsonData);
    }
}