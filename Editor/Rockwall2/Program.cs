using Avalonia;
using MsBox.Avalonia;
using Rockwall2.Editor.Common.Input;
using Silk.NET.SDL;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Rockwall2;
internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
        {
            var requestedName = new System.Reflection.AssemblyName(resolveArgs.Name);
            if (string.Equals(requestedName.Name, "MonoGame.Framework", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(Microsoft.Xna.Framework.Game).Assembly;
            }
            return null;
        };

        AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskSchedulerUnhandledException;

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    private static unsafe void NotifyOfUnhandledException(Exception ex)
    {
        try
        {
            System.IO.File.WriteAllText("c:\\Chisel\\crash.log", ex.ToString());
            System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), ex.ToString());
            Console.Error.WriteLine(ex.ToString());
        }
        catch { }
        if (!Debugger.IsAttached)
        {
            var sdl = Sdl.GetApi();
            sdl.ShowSimpleMessageBox(
                (uint)Silk.NET.SDL.MessageBoxFlags.Error,
                $"Editor Error",
                $"The program has encountered a fatal error and cannot continue.\n{ex.ToString()}",
                null
            );
        }
    }

    public static unsafe void ShowMessageBox(Silk.NET.SDL.MessageBoxFlags flags, string name, string desc)
    {
        var sdl = Sdl.GetApi();
        sdl.ShowSimpleMessageBox(
            (uint)flags,
            name,
            desc,
            null
        );
    }

    private static void CurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        NotifyOfUnhandledException((Exception)e.ExceptionObject);
    }

    private static void TaskSchedulerUnhandledException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        NotifyOfUnhandledException(e.Exception);
        e.SetObserved();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
