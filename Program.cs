using Avalonia;
using System;
using System.Threading;

namespace Maqloub;

sealed class Program
{
    private const string SingleInstanceMutexName =
        "Maqloub_2D2C73E9_8E07_49DB_A71D_F10CB91D095D";

    [STAThread]
    public static void Main(string[] args)
    {
        using var singleInstanceMutex = new Mutex(
            initiallyOwned: true,
            name: SingleInstanceMutexName,
            createdNew: out var isFirstInstance);

        if (!isFirstInstance)
            return;

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}