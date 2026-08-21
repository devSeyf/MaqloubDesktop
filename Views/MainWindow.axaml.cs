using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Diagnostics;
namespace Maqloub.Views;

using Avalonia.Input.Platform;
using Maqloub.Models;
using Maqloub.Services;
using System;
using System.Security.AccessControl;
using System.Threading.Tasks;

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

    public MainWindow()
    {
        InitializeComponent();
        LoadSettings();
        _globalHotkeyService.HotkeyPressed +=GlobalHotkeyService_HotkeyPressed;
      

        Closing += MainWindow_Closing;
    }

    // This method is called when the global hotkey is pressed. It performs the following steps:
    private async void GlobalHotkeyService_HotkeyPressed()
    {
        await Task.Delay(200);
        var copySent = _keyboardService.SendCopyShortcut();

        if (!copySent)
            return;
        await Task.Delay(200);
        var selectedText = await Clipboard.TryGetTextAsync();
        var convertedText = _layoutConverter.Convert(selectedText, _firstLanguage,_secondLanguage);
        Debug.WriteLine($"Original text: {selectedText}");
        Debug.WriteLine($"Converted text: {convertedText}");

        // Place the converted text in the clipboard
        await Clipboard.SetTextAsync(convertedText);
        Debug.WriteLine($"Converted text placed in clipboard: {convertedText}");
        await Task.Delay(150);
        var pasteSent = _keyboardService.SendPasteShortcut();
        Debug.WriteLine($"Paste shortcut sent: {pasteSent}");

 
        if (string.IsNullOrWhiteSpace(selectedText))
        {
            Debug.WriteLine("No selected text was copied.");
            return;
        }
        



        

    }
    private async void StartButton_Click(object? sender, RoutedEventArgs e)
    {
        var firstLanguage =(FirstLanguageComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
       

        var secondLanguage =
            (SecondLanguageComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        if (string.IsNullOrWhiteSpace(firstLanguage) ||
      string.IsNullOrWhiteSpace(secondLanguage))
        {
            Debug.WriteLine("Please select both languages.");
            return;
        }


        // The languages must be different
        if (firstLanguage == secondLanguage)
        {
            Debug.WriteLine("Languages must be different.");
            return;
        }

        _firstLanguage = firstLanguage;
        _secondLanguage = secondLanguage;

        Debug.WriteLine(
            $"Saved languages: {_firstLanguage} <-> {_secondLanguage}");




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

        var settings = new AppSettings
        {
            FirstLanguage = _firstLanguage,
            SecondLanguage = _secondLanguage,
            ShortcutKey = _selectedShortcutKey.Value.ToString(),
            ShortcutModifiers = _selectedShortcutModifiers.ToString()
        };

        await _settingsService.SaveAsync(settings);

        Debug.WriteLine("Settings saved successfully.");

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
    private void MainWindow_Closing(object? sender,WindowClosingEventArgs e)
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

        Debug.WriteLine("Saved settings applied.");
    }





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




    
    private void StartInBackgroundIfConfigured()
    {
        if (_savedSettings is null)
            return;

        Hide();

        Debug.WriteLine(
            "Maqloub started in background.");
    }


}