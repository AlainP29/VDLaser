using System.Threading.Tasks;
using VDLaser.Core.Gcode;
using VDLaser.Core.Gcode.Services;
using VDLaser.Core.Models;

namespace VDLaser.Core.Gcode.Interfaces
{
    public interface IGcodeFileService
    {
        Task<GcodeFileResult> LoadAsync(string filePath);
        //Task SendGCodeAsync(string command);  // Pour streaming vers GRBL via IGrblCoreService
    }
}