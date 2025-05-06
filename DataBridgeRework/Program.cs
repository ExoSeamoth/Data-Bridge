using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using DataBridgeRework.Utils.Models;

namespace DataBridgeRework;

internal abstract class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }



    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions
            {
                RenderingMode =
                [
                    X11RenderingMode.Software,
                    X11RenderingMode.Egl
                ]
            })
            .With(new Win32PlatformOptions
            {
                RenderingMode =
                [
                    Win32RenderingMode.Software,
                    Win32RenderingMode.AngleEgl
                ]
            })
            .WithInterFont()
            .LogToTrace();
    }
}

[JsonSerializable(typeof(ServerConnectionData))]
[JsonSerializable(typeof(List<ServerConnectionData>))]
[JsonSerializable(typeof(ObservableCollection<ServerConnectionData>))]
[JsonSerializable(typeof(IEnumerable<ServerConnectionData>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class AppJsonSerializerContext : JsonSerializerContext;