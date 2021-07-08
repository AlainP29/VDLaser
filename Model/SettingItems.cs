namespace VDLaser.Model
{
    public class SettingItems
    {
        public string SettingHeader { get; private set; }
        public string SettingCode { get; private set; }
        public string SettingValue { get; private set; }
        public string SettingDescription { get; private set; }
        public string Version { get; private set; }
        public string Build { get; private set; }

        public SettingItems()
        {
        
        }
        public SettingItems(string settingHeader)
        {
            SettingHeader = settingHeader;
        }
        public SettingItems(string settingCode, string settingValue, string settingDescription)
        {
            SettingCode = settingCode;
            SettingValue = settingValue;
            SettingDescription = settingDescription;
        }
    }
}
