using Maqloub.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Maqloub.Services;

public sealed class SettingsService
{
    private readonly string _filePath;

    public SettingsService()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Maqloub");
         Directory.CreateDirectory(folder);    
        _filePath = Path.Combine(folder,"settings.json");
    }

    public async Task SaveAsync(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings,new JsonSerializerOptions{WriteIndented = true});
        await File.WriteAllTextAsync(_filePath,json);
    }



    
    
    public async Task<AppSettings?> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return null;

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<AppSettings>(json);
    }
}