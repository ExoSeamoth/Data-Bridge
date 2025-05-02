using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBridgeRework.Utils.Factories;
using DataBridgeRework.Utils.Messages;
using DataBridgeRework.Utils.Models;
using DataBridgeRework.Utils.Services.SftpClientService;
using DataBridgeRework.Views;
using FluentAvalonia.UI.Controls;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace DataBridgeRework.ViewModels;

public sealed partial class MainWindowViewModel : ObservableRecipient
{
    private readonly IExplorerViewModelFactory _tabsFactory;
    private readonly ISftpClientService _sftpClientService;
    [ObservableProperty] private ExplorerViewModel _selectedTab = null!;

    public MainWindowViewModel(IExplorerViewModelFactory tabsFactory, ISftpClientService sftpClientService)
    {
        _tabsFactory = tabsFactory;
        _sftpClientService = sftpClientService;
    }

    public ObservableCollection<ExplorerViewModel> Tabs { get; init; } = new();


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

    public async Task<bool> OpenConnectionDialogAsync(MainWindow window)
    {
        ConnectionWindow connectionWindow = new()
        {
            DataContext = new ConnectionWindowViewModel()
        };

        var connectionData = await connectionWindow.ShowDialog<ServerConnectionData?>(window);

        if (connectionData == null) return false;
        
        return await TryConnectClientAsync(connectionData, window);
    }

    private async Task<bool> TryConnectClientAsync(ServerConnectionData connectionData, MainWindow window)
    {
        try
        {
            AuthenticationMethod authenticationMethod = connectionData.SecurityType switch
            {
                SecurityType.Password => new PasswordAuthenticationMethod(connectionData.UserName,
                    connectionData.Password),
                SecurityType.SshKey => new PrivateKeyAuthenticationMethod(connectionData.UserName,
                    new PrivateKeyFile(connectionData.SshKeyPath, connectionData.SshKeyPhrase)),
                _ => new NoneAuthenticationMethod(connectionData.UserName)
            };

            // AuthenticationMethod authenticationMethod = new NoneAuthenticationMethod(connectionData.UserName);
            //
            // switch (connectionData.SecurityType)
            // {
            //     case SecurityType.Password:
            //         authenticationMethod = new PasswordAuthenticationMethod(connectionData.UserName, connectionData.Password);
            //         break;
            //     case SecurityType.SshKey:
            //         PrivateKeyFile key = new(connectionData.SshKeyPath, connectionData.SshKeyPhrase);
            //         authenticationMethod = new PrivateKeyAuthenticationMethod(connectionData.UserName, key);
            //         break;
            //     default:
            //         new NoneAuthenticationMethod(connectionData.UserName);
            //         break;
            // }

            ConnectionInfo connectionInfo = new(connectionData.HostName, connectionData.Port, connectionData.UserName,
                authenticationMethod);

            await _sftpClientService.ConnectAsync(connectionInfo);
            
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

    private async Task ShowErrorDialogAsync(MainWindow window, string header, string message)
    {
        var dialog = new TaskDialog
        {
            Title = "Ошибка",
            Header = header,
            Content = message,
            XamlRoot = window,
            Buttons = new List<TaskDialogButton> { TaskDialogButton.OKButton },
            IconSource = new BitmapIconSource
                { UriSource = new Uri("avares://DataBridgeRework/Assets/Icons/error.png") }
        };

        await dialog.ShowAsync();
    }
}