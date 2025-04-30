using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using DataBridgeRework.ViewModels;
using DataBridgeRework.Views;

namespace DataBridgeRework;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<Type, Func<Control?>> _locator = new();

    public ViewLocator()
    {
        RegisterViewFactory<MainWindowViewModel, MainWindow>();
        RegisterViewFactory<ExplorerViewModel, ExplorerView>();
        RegisterViewFactory<ConnectionWindowViewModel, ConnectionWindow>();
    }

    public Control Build(object? data)
    {
        if (data is null)
            return new TextBlock { Text = "No VM provided" };

        _locator.TryGetValue(data.GetType(), out var factory);

        return factory?.Invoke() ?? new TextBlock { Text = $"VM Not Registered: {data.GetType()}" };
    }

    public bool Match(object? data)
    {
        return data is ObservableObject;
    }

    public void RegisterViewFactory<TViewModel>(Func<Control> factory) where TViewModel : class
    {
        _locator.Add(typeof(TViewModel), factory);
    }

    public void RegisterViewFactory<TViewModel, TView>()
        where TViewModel : class
        where TView : Control
    {
        _locator.Add(typeof(TViewModel), Ioc.Default.GetService<TView>);
    }
}