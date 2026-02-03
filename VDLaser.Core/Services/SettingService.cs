using System.Threading.Tasks;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;

namespace VDLaser.Core.Services
{
    public class SettingService : ISettingService
    {
        public async Task<SettingItems> GetSettingsAsync()
        {
            var items = new SettingItems();
            // Ton code original : load from file ou GRBL $$$
            await Task.CompletedTask;  // Remplace par async load (ex. : JSON)
            return items;
        }

        public async Task SaveSettingsAsync(SettingItems items)
        {
            // Save to file ou send to GRBL
            await Task.CompletedTask;
        }
    }
}