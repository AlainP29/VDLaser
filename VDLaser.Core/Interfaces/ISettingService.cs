using System.Threading.Tasks;
using VDLaser.Core.Models;

namespace VDLaser.Core.Interfaces
{
    public interface ISettingService
    {
        Task<SoftwareSettings> GetSettingsAsync();
        Task SaveSettingsAsync(SoftwareSettings items);
    }
}