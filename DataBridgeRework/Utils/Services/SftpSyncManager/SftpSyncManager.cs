using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DataBridgeRework.Utils.Models;
using DataBridgeRework.Utils.Services.SftpClientService;
using Renci.SshNet;

namespace DataBridgeRework.Utils.Services.SftpSyncManager;

public sealed class SftpSyncManager : IAsyncDisposable
{
    private SftpClient _client;
    private readonly FileSystemWatcher _watcher = new() { NotifyFilter = NotifyFilters.LastWrite };
    private string _sessionPath = string.Empty;

    private readonly Dictionary<string, string> _watchFiles = [];
    
    public Task StartSessionAsync(ServerConnectionData connectionData, CancellationToken cancellationToken = default)
    {
        _sessionPath = CreateSessionPath(connectionData.Id);
        Directory.CreateDirectory(_sessionPath);
        return Task.CompletedTask;
    }

    public Task StopSessionAsync()
    {
        Directory.Delete(_sessionPath, true);
        return Task.CompletedTask;

    }
    
    private string CreateSessionPath(Guid connectionId) => Directory.GetCurrentDirectory() + @$"\sessions\{connectionId}";
    

    public async ValueTask DisposeAsync()
    {
        if (_client != null) await CastAndDispose(_client);
        if (_watcher != null) await CastAndDispose(_watcher);

        _sessionPath = string.Empty;

        await StopSessionAsync();
        
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