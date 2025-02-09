using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Data_Bridge.ViewModels;
using FluentAvalonia.UI.Controls;
using Renci.SshNet;

namespace Data_Bridge.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        Loaded += OnMainWindowLoaded;
    }

    private async void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            if (!await vm.OpenConnectionDialog(this))
                Close();
            else
                vm.AddTab();
        }
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await vm.OpenConnectionDialog(this);
            vm.Tabs.Clear();
            ExplorerViewModel.Bookmarks.Clear();
            vm.AddTab();
        }
    }

    private void TabView_OnAddTabButtonClick(TabView sender, EventArgs args)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.AddTab();
    }

    private void TabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (DataContext is MainWindowViewModel vm && args.Item is ExplorerViewModel tabToRemove)
        {
            vm.DeleteTab(tabToRemove);
        }

    }

    private void Drag_Window_Event(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}