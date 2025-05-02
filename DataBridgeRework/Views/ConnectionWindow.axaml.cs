using Avalonia.Controls;
using DataBridgeRework.ViewModels;
using FluentAvalonia.UI.Windowing;

namespace DataBridgeRework.Views;

public partial class ConnectionWindow : AppWindow
{
    public ConnectionWindow()
    {
        InitializeComponent();
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        Closing += SaveConnectionsEvent;
    }

    private void SaveConnectionsEvent(object sender, WindowClosingEventArgs e)
    {
        var vm = DataContext as ConnectionWindowViewModel;

        vm.SaveConnectionsToJson();
    }
}