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
        _client = new SftpClient(connectionData);
        return _client.ConnectAsync(cancellationToken);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_client is { IsConnected: true }) return Task.Run(() => _client.Disconnect(), cancellationToken);
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<RemoteFileModel> ListDirectoryAsync(string path,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_client is not { IsConnected: true }) throw new SshConnectionException("Нет активного подключения.");
        await foreach (var remoteFile in _client.ListDirectoryAsync(path, cancellationToken))
        {
            var type = remoteFile switch
            {
                { IsDirectory: true } => FileType.Directory,
                { IsSymbolicLink: true } => FileType.SymbolicLink,
                { IsSocket: true } => FileType.Socket,
                _ => FileType.File
            };

            RemoteFileModel file = new(
                remoteFile.Name,
                remoteFile.FullName,
                remoteFile.Length,
                type,
                remoteFile.LastWriteTime,
                BuildPermissionsString(remoteFile.Attributes)
            );

            yield return file;
        }
    }

    public Task DownloadFileAsync(string remoteFilePath, string localPath, bool overwrite = false,
        IProgress<double> progress = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UploadFileAsync(string localFilePath, string remotePath, bool overwrite = false,
        IProgress<double> progress = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteDirectoryRecursiveAsync(string remoteDirectoryPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CreateDirectoryAsync(string remoteDirectoryPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RenameAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<RemoteFileModel> GetFileInfoAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    private static string BuildPermissionsString(SftpFileAttributes attributes)
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