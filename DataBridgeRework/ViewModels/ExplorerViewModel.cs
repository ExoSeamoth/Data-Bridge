using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBridgeRework.Utils.Entries;
using DataBridgeRework.Utils.Messages;
using DataBridgeRework.Utils.Models;
using DataBridgeRework.Utils.Services.SftpClientService;

namespace DataBridgeRework.ViewModels;

public sealed partial class ExplorerViewModel : ObservableRecipient
{
    private readonly ISftpClientService _sftpClientService;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(GoUpCommand))]
    private string _currentFullPath = string.Empty;

    [ObservableProperty] private string _userHomeDirectory = string.Empty;

    public ExplorerViewModel(ISftpClientService sftpClientService)
    {
        _sftpClientService = sftpClientService;
        UserHomeDirectory = "/home/user/exo";
    }

    public static ObservableCollection<string> Bookmarks =>
    [
        "/etc/passwddddddddddddddddddddddddddddddddddddddddddddd",
        "/etc/hosts",
        "/var/log/syslog",
        "/var/log/auth.log",
        "/home",
        "/home/user/.bashrc",
        "/usr/bin/bash",
        "/usr/local/bin",
        "/tmp",
        "/tmp/testfile.tmp",
        "/opt",
        "/opt/someapp/config.yaml",
        "/root",
        "/root/.ssh/authorized_keys",
        "/boot/vmlinuz",
        "/dev/null",
        "/media/usb",
        "/mnt/data",
        "/srv/ftp",
        "/lib/modules",
        "/proc/cpuinfo",
        "/sys/class/net/eth0",
        "/usr/share/fonts",
        "/var/www/html/index.html"
    ];

    public ObservableCollection<RemoteFileModel> Files { get; } = new();
    public ObservableStack<string> BackHistory { get; } = new();
    public ObservableStack<string> ForwardHistory { get; } = new();

    partial void OnCurrentFullPathChanged(string value)
    {
        LoadFilesCommand.ExecuteAsync(value);
    }

    private void NavigateTo(string fullPath, bool isForward = true)
    {
        if (isForward) BackHistory.Push(CurrentFullPath);
        CurrentFullPath = fullPath;
    }

    [RelayCommand]
    private void CreateNewTab(string fullPath)
    {
        var path = $"/test/folder{Random.Shared.Next(0, 100).ToString()}";
        Messenger.Send(new NewTabMessage(path), CancellationToken.None);
    }

    [RelayCommand]
    private async Task LoadFiles(string path)
    {
        Files.Clear();
        var newFiles = _sftpClientService.ListDirectoryAsync(path);
        await foreach (var file in newFiles) Files.Add(file);
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        var back = BackHistory.Pop();
        NavigateTo(back, false);
    }

    private bool CanGoBack()
    {
        return BackHistory.Count > 0;
    }


    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private void GoForward()
    {
        var forward = ForwardHistory.Pop();
        NavigateTo(forward);
    }

    private bool CanGoForward()
    {
        return ForwardHistory.Count > 0;
    }

    [RelayCommand(CanExecute = nameof(CanGoUp))]
    private void GoUp()
    {
    }

    private bool CanGoUp()
    {
        return CurrentFullPath.Length > 1;
    }
}