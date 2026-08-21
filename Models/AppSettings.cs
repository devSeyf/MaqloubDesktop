namespace Maqloub.Models;

public sealed class AppSettings
{
    public string FirstLanguage { get; set; } = string.Empty;

    public string SecondLanguage { get; set; } = string.Empty;

    public string ShortcutKey { get; set; } = string.Empty;

    public string ShortcutModifiers { get; set; } = string.Empty;
}