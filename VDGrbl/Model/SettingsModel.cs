namespace VDGrbl.Model
{
    public class SettingsModel
    {
        public string SettingsHeader { get; private set; }
        public string SettingCode { get; private set; }
        public string SettingValue { get; private set; }
        public string SettingDescription { get; private set; }

        public SettingsModel()
        { }
        public SettingsModel(string settingsHeader)
        {
            SettingsHeader = settingsHeader;
        }
        public SettingsModel(string settingCode, string settingValue, string settingDescription)
        {
            SettingCode = settingCode;
            SettingValue = settingValue;
            SettingDescription = settingDescription;
        }
    }
}
