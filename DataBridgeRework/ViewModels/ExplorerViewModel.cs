using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBridgeRework.Utils.Entries;
using DataBridgeRework.Utils.Messages;
using DataBridgeRework.Utils.Models;
using DataBridgeRework.Utils.Services.SftpSyncManager;

namespace DataBridgeRework.ViewModels;

public sealed partial class ExplorerViewModel : ObservableRecipient
{
    // private readonly ISftpClientService _sftpClientService;
    private readonly ISftpSyncManager _sftpSyncManager;
    
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(GoUpCommand))] [NotifyCanExecuteChangedFor(nameof(NavigateToCommand))]
    private string _currentFullPath = string.Empty;
    partial void OnCurrentFullPathChanged(string value) => LoadFilesCommand.ExecuteAsync(value);
    
    [ObservableProperty] private string _userHomeDirectory = string.Empty;

    public static ObservableCollection<string> Bookmarks { get; set; } = [];

    public ObservableCollection<RemoteFileModel> Files { get; } = new();

    public ObservableStack<string> BackHistory { get; } = new();

    public ObservableStack<string> ForwardHistory { get; } = new();

    public ExplorerViewModel(ISftpSyncManager sftpSyncManager)
    {
        _sftpSyncManager = sftpSyncManager;
        UserHomeDirectory = _sftpSyncManager.HomeDirectory; 
        CurrentFullPath = UserHomeDirectory;
        BackHistory.CollectionChanged += (_, __) => GoBackCommand.NotifyCanExecuteChanged();
        ForwardHistory.CollectionChanged += (_, __) => GoForwardCommand.NotifyCanExecuteChanged();
        Bookmarks.CollectionChanged += (_, __) => AddBookmarkCommand.NotifyCanExecuteChanged();
        
        _sftpSyncManager.RemoteUpdated += UpdateFiles;
    }

    private async void UpdateFiles(string filePath)
    {
        await Dispatcher.UIThread.InvokeAsync(() => LoadFilesCommand.Execute(CurrentFullPath));
    }

    public async Task OpenRemoteFile(string fullPath)
    {
        await _sftpSyncManager.EditRemoteFileAsync(fullPath);
    }

    [RelayCommand]
    private void OpenFolderInNewTab(string fullPath)
    {
        Messenger.Send(new NewTabMessage(fullPath), CancellationToken.None);
    }

    [RelayCommand]
    private async Task LoadFiles(string path)
    {
        Files.Clear();
        var newFiles = _sftpSyncManager.GetFilesAsync(path);
        await foreach (var file in newFiles) Files.Add(file);
    }

    [RelayCommand(CanExecute = nameof(CanNavigateTo))]
    private void NavigateTo(string fullPath)
    {
        BackHistory.Push(CurrentFullPath);
        ForwardHistory.Clear();
        
        CurrentFullPath = fullPath;
    }
    
    private bool CanNavigateTo(string fullPath) => CurrentFullPath != fullPath;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        var path = BackHistory.Pop();
        ForwardHistory.Push(CurrentFullPath);

        CurrentFullPath = path;
    }

    private bool CanGoBack() => BackHistory.Count > 0;
    
    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private void GoForward()
    {
        var path = ForwardHistory.Pop();
        
        BackHistory.Push(CurrentFullPath);
        
        CurrentFullPath = path;
    }

    private bool CanGoForward() => ForwardHistory.Count > 0;

    [RelayCommand(CanExecute = nameof(CanGoUp))]
    private void GoUp()
    {
        string parentPath = Path.GetDirectoryName(CurrentFullPath)?.Replace("\\", "/") ?? "/";
        BackHistory.Push(CurrentFullPath);

        CurrentFullPath = parentPath;
    }

    private bool CanGoUp() => CurrentFullPath.Length > 1;

    [RelayCommand]
    private void CreateDirectory()
    {
    }

    [RelayCommand]
    private void CreateFile()
    {
        
    }

    [RelayCommand]
    private void CreateSymbolicLink()
    {
        
    }
    
    [RelayCommand(CanExecute = nameof(CanAddBookmark))]
    private void AddBookmark(string fullPath) => Bookmarks.Add(fullPath);
    
    private bool CanAddBookmark(string fullPath) => !Bookmarks.Contains(fullPath);
    
    [RelayCommand]
    private void RemoveBookmark(string fullPath) => Bookmarks.Remove(fullPath);
}