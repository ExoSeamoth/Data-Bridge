using System;

namespace DataBridgeRework.Utils.Models;

public sealed record RemoteFileModel(
    string Name,
    string FullPath,
    long Size,
    FileType Type,
    DateTime LastWriteTime,
    string Permissions
);