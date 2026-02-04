using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;

namespace VDLaser.Core.Services
{
    public class SettingService : ISettingService
    {
        public async Task SaveSettingsAsync(SoftwareSettings items)
        {
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync("settings.json", json);
        }



        public async Task<SoftwareSettings> GetSettingsAsync()
        {
            if (!File.Exists("settings.json"))
                return new SoftwareSettings();

            var json = await File.ReadAllTextAsync("settings.json");
            return JsonSerializer.Deserialize<SoftwareSettings>(json) ?? new SoftwareSettings();
        }

    }
}