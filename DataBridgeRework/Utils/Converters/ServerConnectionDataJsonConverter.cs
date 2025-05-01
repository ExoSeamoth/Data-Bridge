using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DataBridgeRework.Utils.Models;

namespace DataBridgeRework.Utils.Converters;

public sealed class ServerConnectionDataJsonConverter : JsonConverter<ServerConnectionData>
{
    public override ServerConnectionData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        
        var instance = new ServerConnectionData();

        if (root.TryGetProperty("id", out var idProp) && idProp.TryGetGuid(out var guid))
            typeof(ServerConnectionData).GetProperty("Id")?.SetValue(instance, guid);

        if (root.TryGetProperty("userName", out var userName))
            instance.UserName = userName.GetString() ?? "";

        if (root.TryGetProperty("hostName", out var hostName))
            instance.HostName = hostName.GetString() ?? "";

        if (root.TryGetProperty("port", out var port))
            instance.Port = port.GetUInt16();

        if (root.TryGetProperty("type", out var type))
            instance.SecurityType = Enum.TryParse<SecurityType>(type.GetString(), out var secType)
                ? secType
                : SecurityType.Password;

        if (root.TryGetProperty("sshKeyPath", out var keyPath))
            instance.SshKeyPath = keyPath.GetString() ?? "";

        if (root.TryGetProperty("bookmarks", out var bookmarksElem) && bookmarksElem.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in bookmarksElem.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                    instance.Bookmarks.Add(item.GetString()!);
            }
        }

        return instance;
    }

    public override void Write(Utf8JsonWriter writer, ServerConnectionData value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id);
        writer.WriteString("userName", value.UserName);
        writer.WriteString("hostName", value.HostName);
        writer.WriteNumber("port", value.Port);
        writer.WriteString("type", value.SecurityType.ToString());
        writer.WriteString("sshKeyPath", value.SshKeyPath);

        writer.WritePropertyName("bookmarks");
        writer.WriteStartArray();
        foreach (var item in value.Bookmarks)
            writer.WriteStringValue(item);
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}