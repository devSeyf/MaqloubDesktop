using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
 
using Avalonia.Threading;
using Maqloub.ViewModels;
using Maqloub.Views;
using System;
using System.Diagnostics;

namespace Maqloub;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
        }


        base.OnFrameworkInitializationCompleted();
    }



    // This method is called when the
    // "Open Window" button is clicked.
    // It checks if the application lifetime is of type IClassicDesktopStyleApplicationLifetime
    // and if the main window is of type MainWindow. If both conditions are met,
    // it posts a task to the UI thread to show the main window, set its state to normal, and activate it.
    private void OpenWindow_OnClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime
                is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow is not MainWindow mainWindow)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
        });
    }

    // This method is called when the "Exit Application" button is clicked.
    private void ExitApplication_OnClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        if (desktop.MainWindow is MainWindow mainWindow)
        {
            mainWindow.PrepareForShutdown();
        }

        desktop.Shutdown();
    }



    private void OpenSettings_OnClick(
    object? sender,
    EventArgs e)
    {
        if (ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is MainWindow mainWindow)
        {
            mainWindow.Show();
            mainWindow.Activate();

            Debug.WriteLine("Settings window opened.");
            

        }
    }
}