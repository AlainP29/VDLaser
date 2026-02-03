
namespace VDLaser.Core.Models
{
    public partial class SettingItems
    {
        public string SettingHeader { get; private set; } = string.Empty;

        public string SettingCode { get; private set; } = string.Empty;

        public string SettingValue { get; private set; } = string.Empty;

        public string SettingDescription { get; private set; } = string.Empty;

        public string Version { get; private set; } = string.Empty;

        public string Build { get; private set; } = string.Empty;

        public Dictionary<string, string> GrblSettings { get; } = new Dictionary<string, string>();  // Pour $0, $1,

        public SettingItems()
        {
        
        }
        public SettingItems(string header)
        {
            SettingHeader = header;
        }
        public SettingItems(string code, string value, string description)
        {
            SettingCode = code;
            SettingValue = value;
            SettingDescription = description;
        }
    }
}
