using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBridgeRework.Utils.Factories;
using DataBridgeRework.Utils.Messages;
using DataBridgeRework.Utils.Models;
using DataBridgeRework.Views;
using FluentAvalonia.UI.Controls;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace DataBridgeRework.ViewModels;

public sealed partial class MainWindowViewModel : ObservableRecipient
{
    private SftpClient _sftpClient;

    private readonly IExplorerViewModelFactory _tabsFactory;
    [ObservableProperty] private ExplorerViewModel _selectedTab = null!;
    public ObservableCollection<ExplorerViewModel> Tabs { get; init; } = new();

    public MainWindowViewModel(IExplorerViewModelFactory tabsFactory)
    {
        _tabsFactory = tabsFactory;
        AddTabCommand.Execute(null);
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

    public async Task<bool> OpenConnectionDialogAsync(MainWindow window)
    {
        ConnectionWindow connectionWindow = new()
        {
            DataContext = new ConnectionWindowViewModel()
        };

        var client = await connectionWindow.ShowDialog<ServerConnectionData?>(window);

        if (client == null)
            return false;
        //
        // if (!await TryConnectClientAsync(client, window))
        //     return false;
        //
        // _sftpClient = client;
        return true;
    }

    [RelayCommand]
    private void AddTab()
    {
        var newTab = _tabsFactory.Create();
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

    private async Task<bool> TryConnectClientAsync(SftpClient client, MainWindow window)
    {
        try
        {
            client.Connect();
            return true;
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
            Buttons = new List<TaskDialogButton> { TaskDialogButton.OKButton }
            // IconSource = new BitmapIconSource { UriSource = new Uri("avares://Data Bridge/Assets/Icons/error.png") }
        };

        await dialog.ShowAsync();
    }
}