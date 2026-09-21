using Avalonia;
using KFLauncher.Models;
using System;
using System.Linq;

namespace KFLauncher
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static int Main(string[] args)
        {
            Models.TraceLog.Enabled = args.Contains("--trace");
            Models.TraceLog.Log("--- launcher starting ---");

            // avalonia logs to Trace, which nothing is listening to in a windowed app: with
            // --trace send it to the terminal, where a broken binding is worth seeing
            if (Models.TraceLog.Enabled)
            {
                System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
                System.Diagnostics.Trace.AutoFlush = true;
            }

            if (args.Contains("--selftest"))
            {
                return SelfTest.Run();
            }

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return 0;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
