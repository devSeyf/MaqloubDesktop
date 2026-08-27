using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Diagnostics;
namespace Maqloub.Views;

using Avalonia.Input.Platform;
using Avalonia.Media;
using Maqloub.Models;
using Maqloub.Services;
using System;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

public partial class MainWindow : Window
{
    private Key? _selectedShortcutKey;
    private KeyModifiers _selectedShortcutModifiers;
    private readonly WindowsGlobalHotkeyService _globalHotkeyService = new();
    private readonly WindowsKeyboardService _keyboardService = new();

    private string _firstLanguage = string.Empty;
    private string _secondLanguage = string.Empty;

    private readonly KeyboardLayoutConverter _layoutConverter = new();

    // This field is used to control whether the window can be closed.
    private bool _allowClose;

    // This service is used to manage application settings, such as the selected languages and shortcut.
    private readonly SettingsService _settingsService = new();

    // This field holds the saved settings loaded from the settings service.
    private AppSettings? _savedSettings;

    // This service is used to manage clipboard operations, such as saving and restoring text.
    private readonly ClipboardService _clipboardService = new();


    // This service is used to manage application startup behavior, such as enabling or disabling startup with Windows.
    private readonly StartupService _startupService = new();

    public MainWindow()
    {
        InitializeComponent();
        LoadSettings();
        _globalHotkeyService.HotkeyPressed += GlobalHotkeyService_HotkeyPressed;


        Closing += MainWindow_Closing;
    }

    // This method is called when the global hotkey is pressed. It performs the following steps:
    private async void GlobalHotkeyService_HotkeyPressed()
    {
        await Task.Delay(200);

        var clipboard = Clipboard;

        await _clipboardService.SaveAsync(clipboard);

        var copySent = _keyboardService.SendCopyShortcut();

        if (!copySent)
            return;
        await Task.Delay(200);
        var selectedText = await Clipboard.TryGetTextAsync();


        if (string.IsNullOrWhiteSpace(selectedText))
        {
            Debug.WriteLine("No selected text was copied.");
            return;
        }

        var convertedText = _layoutConverter.Convert(selectedText, _firstLanguage, _secondLanguage);
        Debug.WriteLine($"Original text: {selectedText}");
        Debug.WriteLine($"Converted text: {convertedText}");

        // Place the converted text in the clipboard
        await Clipboard.SetTextAsync(convertedText);
        Debug.WriteLine($"Converted text placed in clipboard: {convertedText}");
        await Task.Delay(150);
        var pasteSent = _keyboardService.SendPasteShortcut();
        Debug.WriteLine($"Paste shortcut sent: {pasteSent}");

        await Task.Delay(300);

        await _clipboardService.RestoreAsync(clipboard);

        Debug.WriteLine("Clipboard restored.");

    }
    private async void StartButton_Click(
       object? sender,
       RoutedEventArgs e)
    {
        StatusTextBlock.IsVisible = false;

        var firstLanguage =
            (FirstLanguageComboBox.SelectedItem as ComboBoxItem)
            ?.Content?.ToString();

        var secondLanguage =
            (SecondLanguageComboBox.SelectedItem as ComboBoxItem)
            ?.Content?.ToString();

        if (string.IsNullOrWhiteSpace(firstLanguage) ||
            string.IsNullOrWhiteSpace(secondLanguage))
        {
            ShowStatus(
                "Please select both layouts.",
                Brushes.OrangeRed);

            Debug.WriteLine("Please select both layouts.");
            return;
        }

        if (firstLanguage == secondLanguage)
        {
            ShowStatus(
                "The two layouts must be different.",
                Brushes.OrangeRed);

            Debug.WriteLine("The two layouts must be different.");
            return;
        }

        if (_selectedShortcutKey is null ||
            _selectedShortcutModifiers == KeyModifiers.None)
        {
            ShowStatus(
                "Please choose a valid shortcut.",
                Brushes.OrangeRed);

            Debug.WriteLine("Please choose a valid shortcut.");
            return;
        }

        _firstLanguage = firstLanguage;
        _secondLanguage = secondLanguage;

        Debug.WriteLine(
            $"Saved layouts: {_firstLanguage} <-> {_secondLanguage}");

        Debug.WriteLine(
            $"Shortcut ready: {_selectedShortcutModifiers} + {_selectedShortcutKey}");

        var isRegistered = RegisterCurrentShortcut();

        Debug.WriteLine(
            $"Global hotkey registered successfully: {isRegistered}");

        if (!isRegistered)
        {
            ShowStatus(
                "This shortcut is unavailable. Choose another one.",
                Brushes.OrangeRed);

            Debug.WriteLine(
                "Settings were not saved because hotkey registration failed.");

            return;
        }

        var startWithWindows =
            StartWithWindowsCheckBox.IsChecked == true;

        var startupUpdated = startWithWindows
            ? _startupService.Enable()
            : _startupService.Disable();

        Debug.WriteLine(
            $"Windows startup updated: {startupUpdated}");

        var settings = new AppSettings
        {
            FirstLanguage = _firstLanguage,
            SecondLanguage = _secondLanguage,
            ShortcutKey = _selectedShortcutKey.Value.ToString(),
            ShortcutModifiers = _selectedShortcutModifiers.ToString(),
            StartWithWindows = startWithWindows
        };

        await _settingsService.SaveAsync(settings);

        Debug.WriteLine("Settings saved successfully.");

        ShowStatus(
            "Saved. Maqloub is running in the background.",
            Brushes.LightGreen);

        Debug.WriteLine(
            "Maqloub is now running in the background.");

        await Task.Delay(900);

        Hide();
    }
    // This event handler is triggered when the user presses a key in the ShortcutTextBox.
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


    // This event handler is triggered when the user attempts to close the window.
    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose)
            return;

        e.Cancel = true;
        Hide();

        Debug.WriteLine(
            "Maqloub window hidden. Application still running.");
    }

    public void PrepareForShutdown()
    {
        _allowClose = true;
        _globalHotkeyService.Unregister();

        Debug.WriteLine("Maqloub is ready to shut down.");
    }




    //Loads saved application settings from the settings file
    private async void LoadSettings()
    {
        _savedSettings = await _settingsService.LoadAsync();

        if (_savedSettings is not null)
        {
            Debug.WriteLine($"Loaded: {_savedSettings.FirstLanguage} -> {_savedSettings.SecondLanguage}");
            ApplySavedSettings();
            RegisterSavedHotkey();
            StartInBackgroundIfConfigured();
        }
        else
        {
            Debug.WriteLine("No saved settings found.");
        }

    }




    // This method applies the saved settings to the application, including the selected languages and shortcut key/modifiers.
    private void ApplySavedSettings()
    {
        if (_savedSettings is null)
            return;

        _firstLanguage = _savedSettings.FirstLanguage;
        _secondLanguage = _savedSettings.SecondLanguage;


        SelectComboBoxItem(FirstLanguageComboBox, _firstLanguage);
        SelectComboBoxItem(SecondLanguageComboBox, _secondLanguage);

        if (Enum.TryParse<Key>(
                _savedSettings.ShortcutKey,
                out var key))
        {
            _selectedShortcutKey = key;
        }

        if (Enum.TryParse<KeyModifiers>(
                _savedSettings.ShortcutModifiers,
                out var modifiers))
        {
            _selectedShortcutModifiers = modifiers;
        }

        if (_selectedShortcutKey is not null &&
    _selectedShortcutModifiers != KeyModifiers.None)
        {
            ShortcutTextBox.Text =
                $"{_selectedShortcutModifiers} + {_selectedShortcutKey}";
        }

        StartWithWindowsCheckBox.IsChecked =
    _savedSettings.StartWithWindows;

        Debug.WriteLine("Saved settings applied.");



    }




    //  this method registers the saved hotkey with the global hotkey service, allowing the application to respond to the specified key combination.
    private void RegisterSavedHotkey()
    {
        if (_selectedShortcutKey is null ||
            _selectedShortcutModifiers == KeyModifiers.None)
        {
            Debug.WriteLine("No saved shortcut available.");
            return;
        }



        var platformHandle = TryGetPlatformHandle();

        if (platformHandle is null)
        {
            Debug.WriteLine("Window handle not available.");
            return;
        }

        var registered = _globalHotkeyService.Register(
            platformHandle.Handle,
            _selectedShortcutKey.Value,
            _selectedShortcutModifiers);

        Debug.WriteLine(
            $"Saved hotkey registered: {registered}");
    }




    // This method checks if the application is configured to start in the background and hides the main window if so.
    private void StartInBackgroundIfConfigured()
    {
        if (!HasValidSettings())
        {
            Debug.WriteLine(
                "Settings are incomplete.");

            return;
        }

        Hide();

        Debug.WriteLine(
            "Maqloub started in background.");
    }


    // This method checks if the saved settings are valid,
    // ensuring that all required fields (first language, second language, shortcut key, and shortcut modifiers) are not null or empty.
    private bool HasValidSettings()
    {
        return _savedSettings is not null &&
          !string.IsNullOrWhiteSpace(
              _savedSettings.FirstLanguage) &&
          !string.IsNullOrWhiteSpace(
              _savedSettings.SecondLanguage) &&
          !string.IsNullOrWhiteSpace(
              _savedSettings.ShortcutKey) &&
          !string.IsNullOrWhiteSpace(
              _savedSettings.ShortcutModifiers);
    }



    
    private bool RegisterCurrentShortcut()
    {
        var platformHandle = TryGetPlatformHandle();

        if (platformHandle is null ||
            platformHandle.Handle == IntPtr.Zero)
        {
            Debug.WriteLine("Window handle unavailable.");
            return false;
        }

        _globalHotkeyService.Unregister();

        return _globalHotkeyService.Register(
            platformHandle.Handle,
            _selectedShortcutKey!.Value,
            _selectedShortcutModifiers);
    }



    private static void SelectComboBoxItem(ComboBox comboBox, string savedValue)
    {
        foreach (var item in comboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem &&
                string.Equals(
                    comboBoxItem.Content?.ToString(),
                    savedValue,
                    StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedItem = comboBoxItem;
                return;
            }
        }
    }






    private void ShowStatus(string message, IBrush color)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = color;
        StatusTextBlock.IsVisible = true;

    }









    public void ResetSettings()
    {
        _globalHotkeyService.Unregister();
        _startupService.Disable();
        _settingsService.Delete();

        _savedSettings = null;

        _firstLanguage = string.Empty;
        _secondLanguage = string.Empty;

        _selectedShortcutKey = null;
        _selectedShortcutModifiers = KeyModifiers.None;

        FirstLanguageComboBox.SelectedItem = null;
        SecondLanguageComboBox.SelectedItem = null;

        ShortcutTextBox.Text = string.Empty;
        StartWithWindowsCheckBox.IsChecked = false;
        StatusTextBlock.IsVisible = false;

        Show();
        Activate();

        Debug.WriteLine("Settings reset successfully.");
    }

}