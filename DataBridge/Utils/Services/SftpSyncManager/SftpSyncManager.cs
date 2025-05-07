using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DataBridge.Utils.Models;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace DataBridge.Utils.Services.SftpSyncManager;

public sealed class SftpSyncManager : ISftpSyncManager
{
    private SftpClient _client;
    private readonly FileSystemWatcher _watcher = new() { NotifyFilter = NotifyFilters.Size, IncludeSubdirectories = true };
    private string _sessionPath = string.Empty;
    
    private ILauncher _launcher;
    private IStorageProvider _storageProvider;

    private readonly Dictionary<string, string> _watchFiles = [];
    
    public string HomeDirectory => _client.WorkingDirectory;
    public bool IsSessionExist => _client is not null;

    public event EventHandler<RemoteUpdatedEventArgs> RemoteUpdated;

    public SftpSyncManager()
    {
        _watcher.Changed += SyncFile;
    }
    
    public async Task StartSessionAsync(
        ServerConnectionData connectionData,
        ILauncher launcher,
        IStorageProvider storageProvider,
        CancellationToken cancellationToken = default)
    {
        if (IsSessionExist) throw new SshConnectionException("Current session still exists");
        
        _launcher = launcher;
        _storageProvider = storageProvider;
        
        await ConnectClient(connectionData, cancellationToken);
        
        _sessionPath = CreateSessionPath(connectionData.Id);
        Directory.CreateDirectory(_sessionPath);
        
        _watcher.Path = _sessionPath;
        _watcher.EnableRaisingEvents = true;
    }

    public Task StopSessionAsync(CancellationToken cancellationToken = default)
    {
        if (!IsSessionExist) return Task.CompletedTask;

        return Task.Run(() =>
        {
            _client.Disconnect();
            _client = null;
            _watcher.EnableRaisingEvents = false;
            if (Directory.Exists(_sessionPath)) Directory.Delete(_sessionPath, true);
        }, cancellationToken);
    }

    public async IAsyncEnumerable<RemoteFileModel> GetFilesAsync(string remotePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var remoteFile in _client.ListDirectoryAsync(remotePath, cancellationToken))
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

    public Task CreateDirectoryAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        return _client.CreateDirectoryAsync(remotePath, cancellationToken);
    }

    public async Task CreateFileAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        string localPath = LocalizePath(remotePath);

        Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
        File.OpenWrite(localPath).Close();
        
        var fileInfo = await _storageProvider.TryGetFileFromPathAsync(localPath);
        
        await _launcher.LaunchFileAsync(fileInfo!);
        
        _watchFiles[localPath] = remotePath;
    }

    public Task CreateSymbolicLinkAsync(string remotePath, string linkPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => _client.SymbolicLink(remotePath, linkPath), cancellationToken);
    }

    public Task DeleteDirectoryAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        return _client.DeleteDirectoryAsync(remotePath, cancellationToken);
    }
    
    public Task DeleteFileAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        return _client.DeleteFileAsync(remotePath, cancellationToken);
    }

    public Task DeleteAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        return _client.DeleteAsync(remotePath, cancellationToken);
    }

    public async Task EditRemoteFileAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        var localPath = LocalizePath(remotePath);
        
        if (!Path.Exists(localPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
            await using var output = File.OpenWrite(localPath);
            _client.DownloadFile(remotePath, output);
        }
        
        var fileInfo = await _storageProvider.TryGetFileFromPathAsync(localPath);
        
        await _launcher.LaunchFileAsync(fileInfo!);
        
        _watchFiles[localPath] = remotePath;
    }

    private void SyncFile(object sender, FileSystemEventArgs e)
    {
        var localPath = e.FullPath;
        
        if (!_watchFiles.TryGetValue(localPath, out string remoteFile)) return;

        // TODO: Проблема с чтением файла для синхронизации
        using var file = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        
        _client.UploadFile(file, remoteFile);
        OnFileSynced(remoteFile, e.FullPath);
    }

    private async Task ConnectClient(ServerConnectionData connectionData, CancellationToken cancellationToken = default)
    {
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

        await _client.ConnectAsync(cancellationToken);
    }
    
    private string CreateSessionPath(Guid connectionId)
    {
        string result = Directory.GetCurrentDirectory() + @$"\sessions\{connectionId}";
        Directory.CreateDirectory(result);
        
        return NormalizePath(result);
    }

    private string LocalizePath(string remotePath)
    {
        var combinedPath = Path.Join(
            _sessionPath, 
            remotePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
        return NormalizePath(combinedPath);
    }
    
    private string NormalizePath(string path)
    {
        return Path.GetFullPath(path)
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .TrimEnd(Path.DirectorySeparatorChar);
    }

    private void OnFileSynced(string remotePath, string localPath)
    {
        RemoteUpdated?.Invoke(null, new(remotePath, localPath));
    }

    private static string BuildPermissionsString(SftpFileAttributes attributes)
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

    public async ValueTask DisposeAsync()
    {
        if (_client != null) await CastAndDispose(_client);
        if (_watcher != null) await CastAndDispose(_watcher);

        _sessionPath = string.Empty;
        
        Directory.Delete(_sessionPath, true);
        
        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }
}