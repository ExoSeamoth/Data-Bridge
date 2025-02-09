using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Data_Bridge.Views;
using FluentAvalonia.UI.Controls;
using Renci.SshNet;

namespace Data_Bridge.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private SftpClient _sftpClient;
    [ObservableProperty] private ExplorerViewModel _selectedTab;
    public ObservableCollection<ExplorerViewModel> Tabs { get; } = new();
    
    public void AddTab()
    {
        ExplorerViewModel tab = new(_sftpClient);
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    
    public void DeleteTab(ExplorerViewModel tabItem)
    {
        if (tabItem == SelectedTab)
        {
            Tabs.Remove(tabItem);
            SelectedTab = Tabs.LastOrDefault();
        }
        else
            Tabs.Remove(tabItem);
        
        if (Tabs.Count == 0)
            AddTab();
    }

    public async Task<bool> OpenConnectionDialog(MainWindow window)
    {
        while (true)
        {
            var connectionsDialog = new ConnectionsWindow();
            SftpClient? result = await connectionsDialog.ShowDialog<SftpClient?>(window);

            if (result == null)
                return false;

            try
            {
                result.Connect();
                _sftpClient = result;
                return true;
            }
            catch (Exception exception)
            {
                var exceptionDialog = new TaskDialog()
                {
                    Title = "Ошибка",
                    Header = "Неизвестная ошибка",
                    Content = exception.Message,
                    XamlRoot = window,
                    Buttons = new List<TaskDialogButton> { TaskDialogButton.OKButton },
                    IconSource = new BitmapIconSource{ UriSource = new("avares://Data Bridge/Assets/Icons/error.png")},
                };

                await exceptionDialog.ShowAsync();
            }
        }
    }
}