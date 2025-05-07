using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;
using DataBridge.Utils.Factories;
using DataBridge.Utils.Services.SftpSyncManager;
using DataBridge.ViewModels;
using DataBridge.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DataBridge;

public sealed partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ConfigureViews(services);
            ConfigureViewModels(services);
            var provider = services.BuildServiceProvider();
            Ioc.Default.ConfigureServices(provider);

            var vm = Ioc.Default.GetService<MainWindowViewModel>()!;

            var view = Ioc.Default.GetService<MainWindow>()!;
            view.DataContext = vm;

            desktop.MainWindow = view;
        }
        
        base.OnFrameworkInitializationCompleted();
    }
    
    [Singleton(typeof(SftpSyncManager), typeof(ISftpSyncManager))]
    private static partial void ConfigureServices(IServiceCollection services);

    [Singleton(typeof(MainWindowViewModel))]
    [Transient(typeof(ExplorerViewModel))]
    [Transient(typeof(ConnectionWindowViewModel))]
    [Singleton(typeof(ExplorerViewModelFactory), typeof(IExplorerViewModelFactory))]
    private static partial void ConfigureViewModels(IServiceCollection services);

    [Singleton(typeof(MainWindow))]
    [Transient(typeof(ExplorerView))]
    [Transient(typeof(ConnectionWindow))]
    private static partial void ConfigureViews(IServiceCollection services);
}