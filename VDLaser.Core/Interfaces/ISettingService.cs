using System.Threading.Tasks;
using VDLaser.Core.Models;

namespace VDLaser.Core.Interfaces
{
    public interface ISettingService
    {
        Task<SettingItems> GetSettingsAsync();
        Task SaveSettingsAsync(SettingItems items);
    }
}