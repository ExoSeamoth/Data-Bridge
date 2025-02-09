using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Data_Bridge.ViewModels;

public partial class ExplorerViewModel : ObservableObject
{
    private readonly SftpClient _sftpClient;
    private readonly string _homeDirectory;

    [ObservableProperty] private string _currentDirectoryFullPath = "/";
    [ObservableProperty] private string _lastBackItem = "";
    [ObservableProperty] private string _lastForwardItem = "";
    public static ObservableCollection<ISftpFile> Bookmarks { get; } = [];

    public ObservableCollection<ISftpFile> Files { get; } = [];

    public ObservableCollection<string> BackHistory { get; } = [];
    public ObservableCollection<string> ForwardHistory { get; } = [];


    public ExplorerViewModel(){}

    public ExplorerViewModel(in SftpClient sftpClient)
    {
        _sftpClient = sftpClient;
        _homeDirectory = sftpClient.WorkingDirectory;
        CurrentDirectoryFullPath = _homeDirectory;

        BackHistory.CollectionChanged += (_, __) => GoBackCommand.NotifyCanExecuteChanged();
        ForwardHistory.CollectionChanged += (_, __) => GoForwardCommand.NotifyCanExecuteChanged();

        GetFiles();
    }

    [RelayCommand]
    private void GetFiles()
    {
        var fileList = _sftpClient.ListDirectory(CurrentDirectoryFullPath)
            .Where(item => item.Name != "." && item.Name != "..");

        Files.Clear();
        foreach (var file in fileList)
            Files.Add(file);
    }

    public void NavigateTo(string path, bool isHistoryCall = false)
    {
        if (!isHistoryCall)
        {
            ForwardHistory.Clear();
            LastForwardItem = ForwardHistory.LastOrDefault() ?? "";
            BackHistory.Add(CurrentDirectoryFullPath);
            LastBackItem = BackHistory.LastOrDefault() ?? "";
        }

        CurrentDirectoryFullPath = path;
        GetFiles();
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        var path = BackHistory[^1];
        BackHistory.RemoveAt(BackHistory.Count - 1);
        LastBackItem = BackHistory.LastOrDefault() ?? "";

        ForwardHistory.Add(CurrentDirectoryFullPath);
        LastForwardItem = ForwardHistory.LastOrDefault() ?? "";
        CurrentDirectoryFullPath = path;

        NavigateTo(path, true);
    }

    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private void GoForward()
    {
        var path = ForwardHistory[^1];
        ForwardHistory.RemoveAt(ForwardHistory.Count - 1);
        LastForwardItem = ForwardHistory.LastOrDefault() ?? "";

        BackHistory.Add(CurrentDirectoryFullPath);
        LastBackItem = BackHistory.LastOrDefault() ?? "";
        CurrentDirectoryFullPath = path;

        NavigateTo(path, true);
    }

    [RelayCommand(CanExecute = nameof(CanGoUp))]
    private void GoUp(string? currentDirectory)
    {
        if (CurrentDirectoryFullPath == "/") return;

        string parentPath = Path.GetDirectoryName(CurrentDirectoryFullPath)?.Replace("\\", "/") ?? "/";

        if (!string.IsNullOrEmpty(parentPath))
            NavigateTo(parentPath);
    }

    [RelayCommand]
    private void GoHome()
    {
        if (CurrentDirectoryFullPath != _homeDirectory)
            NavigateTo(_homeDirectory);
    }

    [RelayCommand]
    private void GoToRoot()
    {
        string rootPath = "/";
        if (rootPath != CurrentDirectoryFullPath)
            NavigateTo(rootPath);
    }

    [RelayCommand(CanExecute = nameof(CanAddBookmark))]
    private void AddBookmark(ISftpFile? file)
    {
        Console.WriteLine("AddBookmark");
        Bookmarks.Add(file);
    }

    [RelayCommand]
    private void RemoveBookmark(ISftpFile? file)
    {
        Console.WriteLine("RemoveBookmark");
        Bookmarks.Remove(file);
    }

    public bool CanGoBack() => BackHistory.Count > 0;
    public bool CanGoForward() => ForwardHistory.Count > 0;
    public bool CanGoUp(string? currentDirectory) => currentDirectory != "/";
    public bool CanAddBookmark(ISftpFile? file)
    {
        return file is not null && 
               !Bookmarks.Contains(file) &&
               file.IsDirectory;
    }
}