using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using DataBridge.Utils.Models;

namespace DataBridge.Utils.Services.SftpSyncManager;

public interface ISftpSyncManager : IAsyncDisposable
{
    public string HomeDirectory { get; }

    public bool IsSessionExist { get; }

    public event EventHandler<RemoteUpdatedEventArgs> RemoteUpdated;

    public Task StartSessionAsync(
        ServerConnectionData connectionData,
        ILauncher launcher,
        IStorageProvider storageProvider,
        CancellationToken cancellationToken = default);

    public Task StopSessionAsync(CancellationToken cancellationToken = default);

    public IAsyncEnumerable<RemoteFileModel> GetFilesAsync(
        string remotePath,
        CancellationToken cancellationToken = default);

    public Task CreateDirectoryAsync(string remotePath, CancellationToken cancellationToken = default);

    public Task CreateFileAsync(string remotePath, CancellationToken cancellationToken = default);

    public Task CreateSymbolicLinkAsync(
        string remotePath, 
        string linkPath,
        CancellationToken cancellationToken = default);

    public Task DeleteDirectoryAsync(string remotePath, CancellationToken cancellationToken = default);

    public Task DeleteFileAsync(string remotePath, CancellationToken cancellationToken = default);

    public Task DeleteAsync(string remotePath, CancellationToken cancellationToken = default);

    public Task EditRemoteFileAsync(string remotePath, CancellationToken cancellationToken = default);
}