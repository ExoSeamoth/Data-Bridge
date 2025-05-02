using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DataBridgeRework.Utils.Models;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace DataBridgeRework.Utils.Services.SftpClientService;

public sealed class SftpClientService : ISftpClientService
{
    private SftpClient _client;

    public Task ConnectAsync(ConnectionInfo connectionData, CancellationToken cancellationToken = default)
    {
        if (_client is { IsConnected: true }) return Task.CompletedTask;

        _client = new SftpClient(connectionData);
        
        return _client.ConnectAsync(cancellationToken);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_client is { IsConnected: true }) return Task.Run(() => _client.Disconnect(), cancellationToken);
        
        return Task.CompletedTask;
    }

    public string GetWorkingDirectory()
    {
        if (_client is not { IsConnected: true }) throw new SshConnectionException("Нет активного подключения.");
        
        return _client.WorkingDirectory;
    }
    
    public async IAsyncEnumerable<RemoteFileModel> ListDirectoryAsync(
        string path,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_client is not { IsConnected: true }) throw new SshConnectionException("Нет активного подключения.");
        var remoteFileList = _client.ListDirectoryAsync(path, cancellationToken);
        
        await foreach (var remoteFile in remoteFileList)
        {
            if (remoteFile.Name is "." or "..") continue;
            
            var type = remoteFile switch
            {
                { IsDirectory: true } => FileType.Directory,
                { IsSymbolicLink: true } => FileType.SymbolicLink,
                { IsSocket: true } => FileType.Socket,
                _ => FileType.File
            };

            var size = type switch
            {
                FileType.File => remoteFile.Length,
                _ => -1,
            };
            
            RemoteFileModel file = new(
                remoteFile.Name,
                remoteFile.FullName,
                size,
                type,
                remoteFile.LastWriteTime,
                BuildPermissionsString(remoteFile.Attributes)
            );

            yield return file;
        }
    }

    public Task DownloadFileAsync(
        string remoteFilePath,
        string localPath,
        bool overwrite = false,
        IProgress<double> progress = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UploadFileAsync(
        string localFilePath,
        string remotePath,
        bool overwrite = false,
        IProgress<double> progress = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(
        string remotePath,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteDirectoryRecursiveAsync(
        string remoteDirectoryPath,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool Exists(
        string remotePath)
    {
        if (_client is not { IsConnected: true }) throw new SshConnectionException("Нет активного подключения.");
        
        return _client.Exists(remotePath);
    }

    public Task CreateDirectoryAsync(
        string remoteDirectoryPath,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RenameAsync(
        string oldPath,
        string newPath,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<RemoteFileModel> GetFileInfoAsync(
        string remotePath,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    private static string BuildPermissionsString(in SftpFileAttributes attributes)
    {
        var typeChar = attributes.IsDirectory ? "d" : "-";

        var owner =
            $"{GetPermissionChar(attributes.OwnerCanRead, 'r')}" +
            $"{GetPermissionChar(attributes.OwnerCanWrite, 'w')}" +
            $"{GetPermissionChar(attributes.OwnerCanExecute, 'x')}";

        var group =
            $"{GetPermissionChar(attributes.GroupCanRead, 'r')}" +
            $"{GetPermissionChar(attributes.GroupCanWrite, 'w')}" +
            $"{GetPermissionChar(attributes.GroupCanExecute, 'x')}";

        var others =
            $"{GetPermissionChar(attributes.OthersCanRead, 'r')}" +
            $"{GetPermissionChar(attributes.OthersCanWrite, 'w')}" +
            $"{GetPermissionChar(attributes.OthersCanExecute, 'x')}";

        return $"{typeChar}{owner}{group}{others}";

        char GetPermissionChar(bool canDo, char permissionChar)
        {
            return canDo ? permissionChar : '-';
        }
    }
}