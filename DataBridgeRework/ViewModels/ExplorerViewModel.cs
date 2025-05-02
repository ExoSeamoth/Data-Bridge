using System;
using System.Collections.ObjectModel;
using System.IO;
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
    
    // private string _currentFullPath = string.Empty;
    // public string CurrentFullPath
    // {
    //     get => _currentFullPath;
    //     set
    //     {
    //         if (!_sftpClientService.Exists(value) || _currentFullPath == value)
    //         {
    //             OnPropertyChanged();
    //             return;
    //         }
    //
    //         if (!SetProperty(ref _currentFullPath, value)) return;
    //
    //         GoUpCommand.NotifyCanExecuteChanged();
    //         LoadFilesCommand.ExecuteAsync(value);
    //     }
    // }


    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(GoUpCommand))]
    private string _currentFullPath = string.Empty;
    partial void OnCurrentFullPathChanged(string value) => LoadFilesCommand.ExecuteAsync(value);
    
    [ObservableProperty] private string _userHomeDirectory = string.Empty;

    public static ObservableCollection<string> Bookmarks { get; set; }

    public ObservableCollection<RemoteFileModel> Files { get; } = new();

    public ObservableStack<string> BackHistory { get; } = new();

    public ObservableStack<string> ForwardHistory { get; } = new();

    public ExplorerViewModel(ISftpClientService sftpClientService)
    {
        _sftpClientService = sftpClientService;
        CurrentFullPath = _sftpClientService.GetWorkingDirectory();
        BackHistory.CollectionChanged += (_, __) => GoBackCommand.NotifyCanExecuteChanged();
        ForwardHistory.CollectionChanged += (_, __) => GoForwardCommand.NotifyCanExecuteChanged();
    }

    
    //
    // partial void OnInputPathChanging(string oldValue, string newValue)
    // {
    //     if (oldValue == newValue) return;
    //     
    //     if (!_sftpClientService.Exists(newValue))
    //         InputPath = oldValue;
    // }

    private void NavigateTo(string fullPath)
    {
        BackHistory.Push(CurrentFullPath);
        ForwardHistory.Clear();
        
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
        var path = BackHistory.Pop();
        ForwardHistory.Push(CurrentFullPath);

        CurrentFullPath = path;
        // NavigateTo(path, true);
    }

    private bool CanGoBack() => BackHistory.Count > 0;
    
    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private void GoForward()
    {
        var path = ForwardHistory.Pop();
        
        BackHistory.Push(CurrentFullPath);
        
        CurrentFullPath = path;
        // NavigateTo(path, true);
    }

    private bool CanGoForward() => ForwardHistory.Count > 0;

    [RelayCommand(CanExecute = nameof(CanGoUp))]
    private void GoUp()
    {
        string parentPath = Path.GetDirectoryName(CurrentFullPath)?.Replace("\\", "/") ?? "/";
        NavigateTo(parentPath);
    }

    private bool CanGoUp() => CurrentFullPath.Length > 1;
}