using System;
using System.Collections.Generic;
using System.IO;
using DataBridgeRework.Utils.Services.SftpClientService;

namespace DataBridgeRework.Utils.Services.FileSyncService;

public sealed class FileSyncService : IDisposable
{
    private readonly ISftpClientService _sftpClientService;
    private readonly FileSystemWatcher _watcher;
    private string _watcherPath = Directory.GetCurrentDirectory() + @"\sessions";
    
    private readonly Dictionary<string, string> _watchFiles = new();

    public FileSyncService(ISftpClientService sftpClientService)
    {
        Directory.CreateDirectory(_watcherPath);
        
        _sftpClientService = sftpClientService;
        _watcher = new FileSystemWatcher(_watcherPath)
        {
            NotifyFilter = NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
        };

        _watcher.Changed += SyncFile;
    }

    public void Enable()
    {
        _watcher.EnableRaisingEvents = true;
    }

    public void Disable()
    {
        _watcher.EnableRaisingEvents = false;
    }

    public void AddFileToSync(string localPath, string remotePath)
    {
        _watchFiles[localPath] = remotePath;
    }

    public void RemoveFileFromSync(string localPath)
    {
        _watchFiles.Remove(localPath);
    }
    
    private void SyncFile(object sender, FileSystemEventArgs e)
    {
        if (!_watchFiles.ContainsKey(e.FullPath)) return;
        
        Console.WriteLine($"Sync file: {e.FullPath}");
    }

    public void Dispose()
    {
        Console.WriteLine("Dispose");
        Disable();
        Directory.Delete(_watcherPath, true);
        _watcher.Changed -= SyncFile;
        _watcher.Dispose();
    }
}