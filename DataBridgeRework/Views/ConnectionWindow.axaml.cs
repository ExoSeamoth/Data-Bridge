using FluentAvalonia.UI.Windowing;

namespace DataBridgeRework.Views;

public partial class ConnectionWindow : AppWindow
{
    public ConnectionWindow()
    {
        InitializeComponent();
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
    }
}