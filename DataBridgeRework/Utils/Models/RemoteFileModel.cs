using System;

namespace DataBridgeRework.Utils.Models;

public sealed record RemoteFileModel(
    string Name,
    string FullPath,
    long? Size,
    FileType Type,
    DateTime LastWriteTime,
    string Permissions
): IComparable<RemoteFileModel>
{
    public bool IsDirectory => Type == FileType.Directory;
    public int CompareTo(RemoteFileModel other)
    {
        if(other is null) return 1;
       
        if (Type == FileType.Directory && other.Type != FileType.File)
            return -1;
        if (Type != FileType.File && other.Type == FileType.Directory)
            return 1;

        return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }
}