using System;
using System.IO;

namespace KFLauncher.Models
{
    /// <summary>Errors are always logged, the 250ms ui heartbeat only with --trace.</summary>
    internal static class TraceLog
    {
        private static readonly object Lock = new();
        private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "kflauncher-trace.log");

        public static bool Enabled { get; set; }

        public static void Log(string message)
        {
            if (Enabled)
            {
                Write(message);
            }
        }

        /// <summary>Always written.  An exception that reaches the ui thread used to just freeze the window.</summary>
        public static void Error(string what, Exception ex)
        {
            Write($"ERROR {what}: {ex}");
        }

        private static void Write(string message)
        {
            try
            {
                lock (Lock)
                {
                    File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss.fff} [thread {Environment.CurrentManagedThreadId}] {message}\n");
                }
            }
            catch (IOException)
            {
                // logging must never be the thing that breaks
            }
        }
    }
}
