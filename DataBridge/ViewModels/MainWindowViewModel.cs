using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBridge.Utils.Factories;
using DataBridge.Utils.Messages;
using DataBridge.Utils.Models;
using DataBridge.Utils.Services.SftpSyncManager;
using DataBridge.Views;
using FluentAvalonia.UI.Controls;
using Renci.SshNet.Common;

namespace DataBridge.ViewModels;

public sealed partial class MainWindowViewModel : ObservableRecipient
{
    private readonly IExplorerViewModelFactory _tabsFactory;
    private readonly ISftpSyncManager _sftpSyncManager;
    
    public ObservableCollection<ExplorerViewModel> Tabs { get; init; } = new();
    [ObservableProperty] private ExplorerViewModel _selectedTab = null!;

    public MainWindowViewModel(IExplorerViewModelFactory tabsFactory, ISftpSyncManager sftpSyncManager)
    {
        _tabsFactory = tabsFactory;
        _sftpSyncManager = sftpSyncManager;
    }

    protected override void OnActivated()
    {
        Messenger.Register<MainWindowViewModel, NewTabMessage, CancellationToken>(this, CancellationToken.None,
            (r, m) =>
            {
                var newTab = r._tabsFactory.Create(m.Value);
                r.Tabs.Add(newTab);
            });
    }

    [RelayCommand]
    private void AddTab(ExplorerViewModel? tab)
    {
        var newTab = tab is null ? _tabsFactory.Create() : _tabsFactory.Create(tab.CurrentFullPath);
        Tabs.Add(newTab);
        SelectedTab = newTab;
    }

    [RelayCommand]
    private void RemoveTab(ExplorerViewModel tab)
    {
        var index = Tabs.IndexOf(tab);

        if (tab == SelectedTab)
        {
            if (index > 0) SelectedTab = Tabs[--index];
            else if (index == 0 && Tabs.Count > 1) SelectedTab = Tabs[++index];
        }

        Tabs.Remove(tab);
    }

    [RelayCommand]
    private void ClearTabsExcept(ExplorerViewModel tab)
    {
        var savedTab = _tabsFactory.Create(tab.CurrentFullPath);
        Tabs.Clear();
        Tabs.Add(savedTab);
        SelectedTab = savedTab;
    }

    public void ClearSelf()
    {
        _sftpSyncManager.StopSessionAsync();
    }
    
    public async Task<bool> OpenConnectionDialogAsync(MainWindow window, ILauncher launcher, IStorageProvider storageProvider)
    {
        ConnectionWindow connectionWindow = new()
        {
            DataContext = new ConnectionWindowViewModel()
        };

        var connectionData = await connectionWindow.ShowDialog<ServerConnectionData?>(window);

        if (connectionData == null) return false;
        
        return await TryConnectClientAsync(connectionData, window, launcher, storageProvider);
    }

    private async Task<bool> TryConnectClientAsync(ServerConnectionData connectionData, MainWindow window, ILauncher launcher, IStorageProvider storageProvider)
    {
        try
        {
            if (_sftpSyncManager.IsSessionExist) await _sftpSyncManager.StopSessionAsync();
            await _sftpSyncManager.StartSessionAsync(connectionData, launcher, storageProvider);
            
            Debug.WriteLine("Success connecting to the server");
            
            return true;
        }
        catch (SshAuthenticationException ex)
        {
            await ShowErrorDialogAsync(window, "Ошибка аутентификации", ex.Message);
        }
        catch (SshConnectionException ex)
        {
            await ShowErrorDialogAsync(window, "Ошибка подключения", ex.Message);
        }
        catch (TimeoutException ex)
        {
            await ShowErrorDialogAsync(window, "Таймаут подключения", ex.Message);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync(window, "Неизвестная ошибка", ex.Message);
        }

        Debug.WriteLine("Failed connecting to the server");
        return false;
    }

    public static async Task ShowErrorDialogAsync(MainWindow window, string header, string message)
    {
        var dialog = new TaskDialog
        {
            Title = "Ошибка",
            Header = header,
            Content = message,
            XamlRoot = window,
            Buttons = new List<TaskDialogButton> { TaskDialogButton.OKButton },
            IconSource = new BitmapIconSource
                { UriSource = new Uri("avares://DataBridge/Assets/Icons/error.png") }
        };

        await dialog.ShowAsync();
    }
}