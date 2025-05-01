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
    [JsonPropertyName("id")] 
    public Guid Id { get; private init; } = Guid.NewGuid();
    
    [ObservableProperty] [Required(ErrorMessage = "Введите имя пользователя.")]
    [JsonPropertyName("userName")]
    private string _userName = string.Empty;

    [ObservableProperty] [Required(ErrorMessage = "Введите адрес сервера.")]
    [JsonPropertyName("hostName")]
    private string _hostName = string.Empty;

    [ObservableProperty]
    [JsonPropertyName("port")]
    private ushort _port = 22;

    [ObservableProperty]
    [JsonPropertyName("type")] 
    private SecurityType _securityType = SecurityType.Password;

    [ObservableProperty]
    [JsonIgnore] 
    private string _password = string.Empty;

    [ObservableProperty] [Required(ErrorMessage = "Укажите путь до ключа.")]
    [JsonPropertyName("sshKeyPath")]
    private string _sshKeyPath = string.Empty;

    [ObservableProperty] 
    [JsonIgnore]
    private string _sshKeyPhrase = string.Empty;

    [JsonPropertyName("bookmarks")] 
    public ObservableCollection<string> Bookmarks { get; init; } = [];
    
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