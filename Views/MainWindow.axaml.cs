using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Diagnostics;
namespace Maqloub.Views;

using Avalonia.Input.Platform;
using System.Threading.Tasks;
using Maqloub.Services;
using System;

public partial class MainWindow : Window
{
    private Key? _selectedShortcutKey;
    private KeyModifiers _selectedShortcutModifiers;
    private readonly WindowsGlobalHotkeyService _globalHotkeyService = new();
    private readonly WindowsKeyboardService _keyboardService = new();
    public MainWindow()
    {
        InitializeComponent();
        _globalHotkeyService.HotkeyPressed +=
      GlobalHotkeyService_HotkeyPressed;
    }


    private async void GlobalHotkeyService_HotkeyPressed()
    {
        await Task.Delay(200);

        var copySent = _keyboardService.SendCopyShortcut();

        Debug.WriteLine($"Copy shortcut sent: {copySent}");

        if (!copySent)
            return;

        await Task.Delay(200);

        var selectedText = await Clipboard.TryGetTextAsync();

        if (string.IsNullOrWhiteSpace(selectedText))
        {
            Debug.WriteLine("No selected text was copied.");
            return;
        }

        Debug.WriteLine($"Copied text: {selectedText}");
    }
    private void StartButton_Click(object? sender, RoutedEventArgs e)
    {
        var firstLanguage =
       (FirstLanguageComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        var secondLanguage =
            (SecondLanguageComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        if (string.IsNullOrWhiteSpace(firstLanguage) ||
      string.IsNullOrWhiteSpace(secondLanguage))
        {
            Debug.WriteLine("Please select both languages.");
            return;
        }

        if (firstLanguage == secondLanguage)
        {
            Debug.WriteLine("Languages must be different.");
            return;
        }

        if (_selectedShortcutKey is null ||
    _selectedShortcutModifiers == KeyModifiers.None)
        {
            Debug.WriteLine("Please select a valid shortcut.");
            return;
        }

        Debug.WriteLine(
            $"Shortcut ready: {_selectedShortcutModifiers} + {_selectedShortcutKey}");



        var platformHandle = TryGetPlatformHandle();

        if (platformHandle is null ||
            platformHandle.Handle == IntPtr.Zero)
        {
            Debug.WriteLine("Could not get the native window handle.");
            return;
        }

        Debug.WriteLine(
            $"Native handle type: {platformHandle.HandleDescriptor}");

        var isRegistered = _globalHotkeyService.Register(
            platformHandle.Handle,
            _selectedShortcutKey.Value,
            _selectedShortcutModifiers);

        Debug.WriteLine(
            $"Global hotkey registered successfully: {isRegistered}");

    }

    private void ShortcutTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift
            or Key.LWin or Key.RWin)
        {
            return;
        }

        if (e.KeyModifiers == KeyModifiers.None)
        {
            ShortcutTextBox.Text = "استخدم Ctrl أو Alt أو Shift";
            Debug.WriteLine("Shortcut must contain a modifier.");

            e.Handled = true;
            return;
        }

        _selectedShortcutKey = e.Key;
        _selectedShortcutModifiers = e.KeyModifiers;

        var shortcut = $"{_selectedShortcutModifiers} + {_selectedShortcutKey}";

        ShortcutTextBox.Text = shortcut;

        Debug.WriteLine($"Selected shortcut: {shortcut}");

        e.Handled = true;

     
    }



}