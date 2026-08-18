using Avalonia.Input;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace Maqloub.Services;

public sealed class WindowsGlobalHotkeyService
{
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModWin = 0x0008;
    private const int HotkeyId = 1;
    private const uint ModNoRepeat = 0x4000;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(
        IntPtr windowHandle,
        int hotkeyId,
        uint modifiers,
        uint virtualKey);

    public bool Register(
    IntPtr windowHandle,
    Key key,
    KeyModifiers modifiers)
    {
        if (!OperatingSystem.IsWindows() ||
            windowHandle == IntPtr.Zero)
        {
            Debug.WriteLine("Windows window handle is not available.");
            return false;
        }

        var windowsModifiers =
            ConvertModifiers(modifiers) | ModNoRepeat;

        var virtualKey = ConvertKey(key);

        if (windowsModifiers == 0 || virtualKey == 0)
        {
            Debug.WriteLine("Shortcut conversion failed.");
            return false;
        }

        if (!InstallMessageHook(windowHandle))
            return false;

        var isRegistered = RegisterHotKey(
            windowHandle,
            HotkeyId,
            windowsModifiers,
            virtualKey);

        if (!isRegistered)
        {
            var errorCode = Marshal.GetLastWin32Error();

            Debug.WriteLine(
                $"Global hotkey registration failed. Error: {errorCode}");

            return false;
        }

        Debug.WriteLine(
            $"Global hotkey registered: {modifiers} + {key}");

        return true;
    }
    private static uint ConvertModifiers(KeyModifiers modifiers)
    {
        uint result = 0;

        if (modifiers.HasFlag(KeyModifiers.Alt))
            result |= ModAlt;

        if (modifiers.HasFlag(KeyModifiers.Control))
            result |= ModControl;

        if (modifiers.HasFlag(KeyModifiers.Shift))
            result |= ModShift;

        if (modifiers.HasFlag(KeyModifiers.Meta))
            result |= ModWin;

        return result;
    }

    private static uint ConvertKey(Key key)
    {
        var keyName = key.ToString();

        if (keyName.Length == 1 && char.IsLetter(keyName[0]))
            return char.ToUpperInvariant(keyName[0]);

        if (keyName.Length == 2 &&
            keyName[0] == 'D' &&
            char.IsDigit(keyName[1]))
        {
            return keyName[1];
        }

        return 0;
    }

    public bool CheckPlatformAndShortcut(Key key,    KeyModifiers modifiers)
    {
        if (!OperatingSystem.IsWindows())
        {
            Debug.WriteLine(
                "Global hotkey is currently supported only on Windows.");

            return false;
        }

        var windowsModifiers = ConvertModifiers(modifiers);
        var virtualKey = ConvertKey(key);

        Debug.WriteLine(
            $"Avalonia shortcut: {modifiers} + {key}");

        Debug.WriteLine(
            $"Windows modifiers code: {windowsModifiers}");

        Debug.WriteLine(
            $"Windows virtual-key code: 0x{virtualKey:X2}");

        return windowsModifiers != 0 && virtualKey != 0;
    }



    private const uint WmHotkey = 0x0312;
    private const int GwlWndProc = -4;

    private IntPtr _previousWindowProcedure;
    private WindowProcedure? _windowProcedure;

    public event Action? HotkeyPressed;

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr WindowProcedure(
        IntPtr windowHandle,
        uint message,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport(
        "user32.dll",
        EntryPoint = "SetWindowLongPtrW",
        SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(
        IntPtr windowHandle,
        int index,
        IntPtr newValue);

    [DllImport(
        "user32.dll",
        EntryPoint = "CallWindowProcW")]
    private static extern IntPtr CallWindowProc(
        IntPtr previousWindowProcedure,
        IntPtr windowHandle,
        uint message,
        IntPtr wParam,
        IntPtr lParam);



    private bool InstallMessageHook(IntPtr windowHandle)
    {
        if (_windowProcedure is not null)
            return true;

        _windowProcedure = WindowMessageHandler;

        var procedurePointer =
            Marshal.GetFunctionPointerForDelegate(_windowProcedure);

        _previousWindowProcedure = SetWindowLongPtr(
            windowHandle,
            GwlWndProc,
            procedurePointer);

        if (_previousWindowProcedure == IntPtr.Zero)
        {
            var errorCode = Marshal.GetLastWin32Error();

            Debug.WriteLine(
                $"Message hook failed. Error: {errorCode}");

            _windowProcedure = null;
            return false;
        }

        Debug.WriteLine("Windows message hook installed.");
        return true;
    }

    private IntPtr WindowMessageHandler(
        IntPtr windowHandle,
        uint message,
        IntPtr wParam,
        IntPtr lParam)
    {
        if (message == WmHotkey &&
            wParam.ToInt32() == HotkeyId)
        {
            Debug.WriteLine("WM_HOTKEY received.");

            HotkeyPressed?.Invoke();

            return IntPtr.Zero;
        }

        return CallWindowProc(
            _previousWindowProcedure,
            windowHandle,
            message,
            wParam,
            lParam);
    }
}