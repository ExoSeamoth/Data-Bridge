using System.Text.Json.Serialization;

namespace DataBridgeRework.Utils.Models;

public enum SecurityType
{
    [JsonPropertyName("password")] Password,
    [JsonPropertyName("sshKey")] SshKey
}