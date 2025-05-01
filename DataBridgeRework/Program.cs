using System;
using Avalonia;

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