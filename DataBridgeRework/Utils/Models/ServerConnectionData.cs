using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DataBridgeRework.Utils.Converters;

namespace DataBridgeRework.Utils.Models;

[JsonConverter(typeof(ServerConnectionDataJsonConverter))]
public partial class ServerConnectionData : ObservableValidator
{
    public Guid Id { get; private init; } = Guid.NewGuid();

    [ObservableProperty] [Required(ErrorMessage = "Введите имя пользователя.")] 
    private string _userName = string.Empty;

    [ObservableProperty] [Required(ErrorMessage = "Введите адрес сервера.")]
    private string _hostName = string.Empty;

    [ObservableProperty]
    private ushort _port = 22;

    [ObservableProperty]
    private SecurityType _securityType = SecurityType.Password;

    [ObservableProperty] 
    private string _password = string.Empty;

    [ObservableProperty] [Required(ErrorMessage = "Укажите путь до ключа.")]
    private string _sshKeyPath = string.Empty;

    [ObservableProperty] private string _sshKeyPhrase = string.Empty;

    public ObservableCollection<string> Bookmarks { get; private set; } = [];

    public void ValidateFields()
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