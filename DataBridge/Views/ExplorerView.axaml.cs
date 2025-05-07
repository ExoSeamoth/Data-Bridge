using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using DataBridge.Utils.Models;
using DataBridge.ViewModels;

namespace DataBridge.Views;

public partial class ExplorerView : UserControl
{
    public ExplorerView()
    {
        InitializeComponent();
    }

    private async void FilesPresenter_SelectedFolderClicked(object sender, DataGridCellPointerPressedEventArgs e)
    {
        var pointer = e.PointerPressedEventArgs.GetCurrentPoint(null);

        if (e.PointerPressedEventArgs.ClickCount != 2 || !pointer.Properties.IsLeftButtonPressed) return;
        
        var vm = (DataContext as ExplorerViewModel)!;

        var remoteFile = (e.Row.DataContext as RemoteFileModel)!;

        switch (remoteFile.Type)
        {
            case FileType.Directory:
                vm.NavigateToCommand.Execute(remoteFile.FullPath);
                break;
            case FileType.File:
                await vm.OpenRemoteFile(remoteFile.FullPath);
                Debug.WriteLine("DoubleClick on file");
                break;
            case FileType.SymbolicLink:
                Debug.WriteLine("DoubleClick on symbolic link");
                break;
            case FileType.Socket:
                Debug.WriteLine("DoubleClick on socket");
                break;
            default: return;
        }
    }

    private void FilesPresenter_OnTemplateApplied(object sender, TemplateAppliedEventArgs e)
    {
        var dataGrid = (sender as DataGrid)!;
        dataGrid.Columns.First().Sort();
        dataGrid.TemplateApplied -= FilesPresenter_OnTemplateApplied;
    }
}