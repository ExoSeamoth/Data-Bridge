using System;
using System.Collections.Generic;
using System.IO;
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

    public Guid ConnectionId { get; private set; }

    public Task ConnectAsync(ServerConnectionData connectionData, CancellationToken cancellationToken = default)
    {
        if (_client is { IsConnected: true }) return Task.CompletedTask;

        ConnectionId = connectionData.Id;

        AuthenticationMethod authenticationMethod = connectionData.SecurityType switch
        {
            SecurityType.Password => new PasswordAuthenticationMethod(connectionData.UserName,
                connectionData.Password),
            SecurityType.SshKey => new PrivateKeyAuthenticationMethod(connectionData.UserName,
                new PrivateKeyFile(connectionData.SshKeyPath, connectionData.SshKeyPhrase)),
            _ => new NoneAuthenticationMethod(connectionData.UserName)
        };

        ConnectionInfo connectionInfo = new(connectionData.HostName, connectionData.Port, connectionData.UserName,
            authenticationMethod);

        _client = new SftpClient(connectionInfo);

        return _client.ConnectAsync(cancellationToken);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_client is not { IsConnected: true }) return Task.CompletedTask;

        ConnectionId = Guid.Empty;
        return Task.Run(() => _client.Disconnect(), cancellationToken);
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

            long? size = type switch
            {
                FileType.File => remoteFile.Length,
                _ => null,
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
        if (_client is not { IsConnected: true }) throw new SshConnectionException("Нет активного подключения.");
        
        FileStream fileStream = new(localPath, FileMode.Create);
        
        return Task.Run(() => _client.DownloadFile(remoteFilePath, fileStream), cancellationToken);
    }

    public Task UploadFileAsync(
        string localFilePath,
        string remotePath,
        bool overwrite = false,
        IProgress<double> progress = null,
        CancellationToken cancellationToken = default)
    {
        if (_client is not { IsConnected: true }) throw new SshConnectionException("Нет активного подключения.");

        FileStream fileStream = new(localFilePath, FileMode.Open);
        
        return Task.Run(() => _client.UploadFile(fileStream, remotePath), cancellationToken);
    }

    public Task DeleteAsync(
        string remotePath,
        CancellationToken cancellationToken = default)
    {
        if (_client is not { IsConnected: true }) throw new SshConnectionException("Нет активного подключения.");

        return _client.DeleteAsync(remotePath, cancellationToken);
    }

    public Task DeleteDirectoryRecursiveAsync(
        string remoteDirectoryPath,
        CancellationToken cancellationToken = default)
    {
        if (_client is not { IsConnected: true }) throw new SshConnectionException("Нет активного подключения.");

        return _client.DeleteDirectoryAsync(remoteDirectoryPath, cancellationToken);
    }

    public bool Exists(
        string remotePath)
    {
        if (_client is not { IsConnected: true }) throw new SshConnectionException("Нет активного подключения.");

        return _client.Exists(remotePath);
    }

    public Task CreateDirectoryAsync(string remoteDirectoryPath, CancellationToken cancellationToken = default)
    {
        if (_client is not { IsConnected: true }) throw new SshConnectionException("Нет активного подключения.");

        return _client.CreateDirectoryAsync(remoteDirectoryPath, cancellationToken);
    }

    public Task CreateFileAsync(string remoteFilePath, CancellationToken cancellationToken = default)
    {
        if (_client is not { IsConnected: true }) throw new SshConnectionException("Нет активного подключения.");

        return Task.Run(() => _client.Create(remoteFilePath), cancellationToken);
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
        var typeChar = attributes switch
        {
            { IsDirectory: true } => 'd',
            { IsSymbolicLink: true } => 'l',
            { IsSocket: true } => 's',
            _ => '-'
        };

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