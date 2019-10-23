using System;

namespace VDGrbl.Model
{
    public interface ISettingsService
    {
        void GetSettings(Action<SettingItem, Exception> callback);
    }
}
