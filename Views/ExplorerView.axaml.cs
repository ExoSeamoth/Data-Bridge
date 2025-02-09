using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Data_Bridge.ViewModels;
using Renci.SshNet.Sftp;

namespace Data_Bridge.Views;

public partial class ExplorerView : UserControl
{
    
    public ExplorerView()
    {
        if (Design.IsDesignMode)
        {
            Design.SetDataContext(this, new ExplorerViewModel());
        }
        InitializeComponent();
    }

    private void DataGrid_OnCellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        var pointer = e.PointerPressedEventArgs.GetCurrentPoint(null);
        if (DataContext is ExplorerViewModel vm && e.PointerPressedEventArgs.ClickCount == 2 && pointer.Properties.IsLeftButtonPressed)
        {
            if (e.Row.DataContext is ISftpFile sftpFile)
                if (sftpFile.IsDirectory)
                    vm.NavigateTo(sftpFile.FullName);
                else
                {
                    Console.WriteLine($"Open file {sftpFile.FullName}");
                }
        }
    }

    private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        //TODO: Доделать логику строки пути
        if (DataContext is ExplorerViewModel vm)
        {
            string oldValue = (string)e.OldValue;
            try
            {
                vm.GetFilesCommand.Execute(null);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}