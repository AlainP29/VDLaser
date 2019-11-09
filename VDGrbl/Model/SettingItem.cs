namespace VDLaser.Model
{
    public class SettingItem
    {
        public string SettingHeader { get; private set; }
        public string SettingCode { get; private set; }
        public string SettingValue { get; private set; }
        public string SettingDescription { get; private set; }
        public string Version { get; private set; }
        public string Build { get; private set; }

        public SettingItem()
        { }
        public SettingItem(string settingHeader)
        {
            SettingHeader = settingHeader;
        }
        public SettingItem(string settingCode, string settingValue, string settingDescription)
        {
            SettingCode = settingCode;
            SettingValue = settingValue;
            SettingDescription = settingDescription;
        }
    }
}
