using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DataBridgeRework.ViewModels;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;

namespace DataBridgeRework.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
        if (!Design.IsDesignMode) Loaded += OnMainWindowLoaded;
        if (!Design.IsDesignMode) Closing += OnMainWindowClosing;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
    }


    private void TabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        var tab = args.Item as ExplorerViewModel;
        (DataContext as MainWindowViewModel)?.RemoveTabCommand.Execute(tab);
    }

    private async void OnMainWindowLoaded(object? sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        
        if (await vm.OpenConnectionDialogAsync(this, Launcher, StorageProvider))
        {
            Debug.WriteLine("Successfully open connection dialog from start");
            vm.IsActive = true;
            vm.AddTabCommand.Execute(null);
        }
        else
        {
            Debug.WriteLine("Failed to open connection dialog from start");
            Close();
        }
    }

    private void OnMainWindowClosing(object sender, WindowClosingEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        
        vm.ClearSelf();
    }

    private async void OpenConnectionDialogRequest(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;

        if (!await vm.OpenConnectionDialogAsync(this, Launcher, StorageProvider))
        {
            Debug.WriteLine("Failed to open connection dialog");
        }
        else
        {
            Debug.WriteLine("Successfully open connection dialog");
            // TODO: В планах добавить сохранение информации про закладки
            vm.Tabs.Clear();
            vm.AddTabCommand.Execute(null);
        }
    }
}