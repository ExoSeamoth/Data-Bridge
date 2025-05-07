using System;

namespace DataBridge.Utils.Models;

public sealed class RemoteUpdatedEventArgs(string remotePath, string localPath) : EventArgs
{
    public string RemotePath { get; } = remotePath;
    public string LocalPath { get; } = localPath;
}