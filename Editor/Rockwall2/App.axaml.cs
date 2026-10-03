using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Rockwall2.Editor.Common;
using Rockwall2.ViewModels;
using Rockwall2.Views;
using System.Linq;
using System.Threading.Tasks;

namespace Rockwall2;
public partial class App : Application
{
    public static EditorHost Host { get; private set; } = null!;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        Host = new EditorHost();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            bool openDirectly = false;
            if (desktop.Args != null && desktop.Args.Length > 0)
            {
                string arg = desktop.Args[0];
                if (System.IO.File.Exists(arg))
                {
                    ConfigManager.LoadConfig(arg);
                    openDirectly = true;
                }
                else
                {
                    ConfigManager.LoadAllConfigs();
                    if (ConfigManager.configFiles != null)
                    {
                        var matching = ConfigManager.configFiles.FirstOrDefault(c =>
                            System.IO.Path.GetFileNameWithoutExtension(c).Equals(arg, System.StringComparison.OrdinalIgnoreCase));
                        if (matching != null)
                        {
                            ConfigManager.LoadConfig(matching);
                            openDirectly = true;
                        }
                    }
                }
            }

            if (!openDirectly)
            {
                var setup = new StartWindow();
                desktop.MainWindow = setup;
                setup.Show();

                var tcs = new TaskCompletionSource<bool>();
                setup.Closed += (_, _) => tcs.TrySetResult(setup.Result == true);
                var success = await tcs.Task;

                if (!success)
                {
                    desktop.Shutdown();
                    return;
                }
            }

            var main = new MainWindow();
            desktop.MainWindow = main;
            main.Show();
            main.InitMainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}