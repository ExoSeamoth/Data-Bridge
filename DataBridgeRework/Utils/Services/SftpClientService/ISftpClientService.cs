using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DataBridgeRework.Utils.Models;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace DataBridgeRework.Utils.Services.SftpClientService;

public interface ISftpClientService : IAsyncDisposable
{
    public Guid ConnectionId { get; }
    
    Task ConnectAsync(ServerConnectionData connectionData, CancellationToken cancellationToken = default);
    
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    string GetWorkingDirectory();
    
    IAsyncEnumerable<RemoteFileModel> ListDirectoryAsync(string path, CancellationToken cancellationToken = default);
    
    Task DownloadFileAsync(
        string remoteFilePath,
        string localPath,
        bool overwrite = false,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
    
    Task UploadFileAsync(
        string localFilePath,
        string remotePath,
        bool overwrite = false,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
    
    Task DeleteAsync(string remotePath, CancellationToken cancellationToken = default);
    
    Task DeleteDirectoryRecursiveAsync(string remoteDirectoryPath, CancellationToken cancellationToken = default);
    
    bool Exists(string remotePath);
    
    Task CreateDirectoryAsync(string remoteDirectoryPath, CancellationToken cancellationToken = default);

    Task CreateFileAsync(string remoteFilePath, CancellationToken cancellationToken = default);

    Task RenameAsync(string oldPath, string newPath, CancellationToken cancellationToken = default);
    
    Task<RemoteFileModel> GetFileInfoAsync(string remotePath, CancellationToken cancellationToken = default);
}