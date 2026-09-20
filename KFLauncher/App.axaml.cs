using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using KFLauncher.Models;
using KFLauncher.ViewModels;
using KFLauncher.Views;

namespace KFLauncher
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // an unhandled exception on the ui thread kills the dispatcher loop, which looks exactly
            // like a frozen window: alive process, nothing redrawing.  log it and carry on instead.
            Dispatcher.UIThread.UnhandledException += (_, e) =>
            {
                TraceLog.Error("unhandled on the ui thread", e.Exception);
                e.Handled = true;
            };

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
