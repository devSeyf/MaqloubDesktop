using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace Maqloub.Services;

public sealed class StartupService
{
    private const string AppName = "Maqloub";
    private const string RunKeyPath =@"Software\Microsoft\Windows\CurrentVersion\Run";
        

    public bool Enable()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        var executablePath = Environment.ProcessPath;

        if (string.IsNullOrWhiteSpace(executablePath))
            return false;

        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath,writable: true);
        if (key is null)
            return false;

        key.SetValue(AppName,$"\"{executablePath}\"");
        Debug.WriteLine($"Windows startup enabled: {executablePath}");
 
        return true;
    }

    public bool Disable()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath,writable: true);
            
        if (key is null)
            return false;

        key.DeleteValue(AppName,throwOnMissingValue: false);
        Debug.WriteLine("Windows startup disabled.");
        return true;
    }

    public bool IsEnabled()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        using var key = Registry.CurrentUser.OpenSubKey(
            RunKeyPath,
            writable: false);

        return key?.GetValue(AppName) is string;
    }
}