using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DataBridgeRework.Utils.Models;
using Renci.SshNet;

namespace DataBridgeRework.Utils.Services.SftpClientService;

public sealed class FakeSftpClientService : ISftpClientService
{
    private readonly Dictionary<string, FakeRemoteFile> _fakeFileSystem = new();
    private bool _isConnected;

    public Task ConnectAsync(ConnectionInfo connectionData, CancellationToken cancellationToken = default)
    {
        _isConnected = true;
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _isConnected = false;
        return Task.CompletedTask;
    }

    public string GetWorkingDirectory()
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<RemoteFileModel> ListDirectoryAsync(string path,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var files = _fakeFileSystem.Values
            .Where(f => Path.GetDirectoryName(f.Path)?.Replace('\\', '/') == path.TrimEnd('/'))
            .ToList();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return new RemoteFileModel(
                Path.GetFileName(file.Path),
                file.Path,
                file.Size,
                file.IsDirectory ? FileType.Directory : FileType.File,
                file.LastWriteTime,
                file.Permissions
            );
        }

        await Task.CompletedTask;
    }

    public Task DownloadFileAsync(string remoteFilePath, string localPath, bool overwrite = false,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        if (!_fakeFileSystem.TryGetValue(remoteFilePath, out var fakeFile))
            throw new FileNotFoundException("Файл не найден в фейковой файловой системе.", remoteFilePath);

        if (fakeFile.IsDirectory) throw new IOException("Нельзя скачать директорию как файл.");

        if (File.Exists(localPath) && !overwrite)
            throw new IOException("Файл уже существует локально и перезапись запрещена.");

        File.WriteAllText(localPath, fakeFile.Content ?? string.Empty);
        progress?.Report(100);

        return Task.CompletedTask;
    }

    public Task UploadFileAsync(string localFilePath, string remotePath, bool overwrite = false,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        if (!File.Exists(localFilePath)) throw new FileNotFoundException("Локальный файл не найден.", localFilePath);

        if (!overwrite && _fakeFileSystem.ContainsKey(remotePath))
            throw new IOException("Файл уже существует на сервере и перезапись запрещена.");

        var content = File.ReadAllText(localFilePath);

        _fakeFileSystem[remotePath] = new FakeRemoteFile
        {
            Path = remotePath,
            IsDirectory = false,
            Content = content,
            Size = content.Length,
            LastWriteTime = DateTime.UtcNow,
            Permissions = "rw-r--r--"
        };

        progress?.Report(100);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        _fakeFileSystem.Remove(remotePath);
        return Task.CompletedTask;
    }

    public async Task DeleteDirectoryRecursiveAsync(string remoteDirectoryPath,
        CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var keysToDelete = _fakeFileSystem.Keys
            .Where(k => k.StartsWith(remoteDirectoryPath.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase) ||
                        k.Equals(remoteDirectoryPath, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToDelete)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _fakeFileSystem.Remove(key);
        }

        await Task.CompletedTask;
    }

    public bool Exists(string remotePath)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        return Task.FromResult(_fakeFileSystem.ContainsKey(remotePath));
    }

    public Task CreateDirectoryAsync(string remoteDirectoryPath, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        if (_fakeFileSystem.ContainsKey(remoteDirectoryPath)) throw new IOException("Директория уже существует.");

        _fakeFileSystem[remoteDirectoryPath] = new FakeRemoteFile
        {
            Path = remoteDirectoryPath,
            IsDirectory = true,
            Size = 0,
            LastWriteTime = DateTime.UtcNow,
            Permissions = "rwxr-xr-x"
        };

        return Task.CompletedTask;
    }

    public Task RenameAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        if (!_fakeFileSystem.TryGetValue(oldPath, out var file))
            throw new FileNotFoundException("Файл или директория для переименования не найдены.", oldPath);

        _fakeFileSystem.Remove(oldPath);
        file.Path = newPath;
        _fakeFileSystem[newPath] = file;

        return Task.CompletedTask;
    }

    public Task<RemoteFileModel> GetFileInfoAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        if (!_fakeFileSystem.TryGetValue(remotePath, out var file))
            throw new FileNotFoundException("Файл не найден.", remotePath);

        return Task.FromResult(new RemoteFileModel(
            Path.GetFileName(file.Path),
            file.Path,
            file.Size,
            file.IsDirectory ? FileType.Directory : FileType.File,
            file.LastWriteTime,
            file.Permissions
        ));
    }

    public ValueTask DisposeAsync()
    {
        _isConnected = false;
        _fakeFileSystem.Clear();
        return ValueTask.CompletedTask;
    }

    private void EnsureConnected()
    {
        if (!_isConnected) throw new InvalidOperationException("Нет активного подключения.");
    }

    private class FakeRemoteFile
    {
        public string Path { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string Permissions { get; set; } = "rw-r--r--";
        public string? Content { get; set; }
    }
}