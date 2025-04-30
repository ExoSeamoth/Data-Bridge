using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace DataBridgeRework.Utils.Models;

public sealed class ServerConnectionData
{
    [JsonPropertyName("id")] public Guid Id { get; init; } = Guid.NewGuid();

    [JsonPropertyName("serverName")] public string ServerName { get; set; } = "Новый сервер";

    [JsonPropertyName("userName")] public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("hostName")] public string HostName { get; set; } = string.Empty;

    [JsonPropertyName("port")] public ushort Port { get; set; } = 22;

    [JsonPropertyName("type")] public SecurityType Type { get; set; } = SecurityType.Password;

    [JsonIgnore] public string Password { get; set; } = string.Empty;

    [JsonPropertyName("sshKeyPath")] public string SshKeyPath { get; set; } = string.Empty;

    [JsonIgnore] public string SshKeyPhrase { get; set; } = string.Empty;

    [JsonPropertyName("bookmarks")] public ObservableCollection<string> Bookmarks { get; init; } = [];
}