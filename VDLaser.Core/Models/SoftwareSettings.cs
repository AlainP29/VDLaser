
namespace VDLaser.Core.Models
{
    /// <summary>
    /// Software settings and preferences
    /// </summary>
    public class SoftwareSettings
    {
        // General software preferences
        public string LastUsedPort { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 115200;

        // UI preferences
        public bool DarkMode { get; set; } = false;
        public bool AutoScrollConsole { get; set; } = true;

        // Laser defaults
        public int DefaultLaserPower { get; set; } = 1000;
        public int DefaultFeedRate { get; set; } = 3000;

        // Paths
        public string LastOpenedFolder { get; set; } = string.Empty;

        // Future expansion
        public Dictionary<string, string> CustomSettings { get; set; } = new();
    }
}
