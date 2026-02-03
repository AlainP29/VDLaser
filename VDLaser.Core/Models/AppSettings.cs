
namespace VDLaser.Core.Models
{
    public partial class AppSettings
    {
        public string StringSetting { get; set; } = string.Empty;
        public int IntegerSetting { get; set; } = 0;
        public bool BooleanSetting { get; set; } = false;
        public AppSettings()
        {

        }
    }
}
