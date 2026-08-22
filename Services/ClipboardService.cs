using Avalonia.Input.Platform;
using System.Threading.Tasks;

namespace Maqloub.Services;

public sealed class ClipboardService
{
    private string? _savedText;

    public async Task SaveAsync(IClipboard clipboard)
    {
        _savedText = await clipboard.TryGetTextAsync();
    }


    public async Task RestoreAsync(IClipboard clipboard)
    {
        if (_savedText is null)
            return;

        await clipboard.SetTextAsync(_savedText);
    }
}