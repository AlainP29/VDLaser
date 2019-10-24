using System;

namespace VDGrbl.Model
{
    public interface ISettingService
    {
        void GetSettings(Action<SettingItem, Exception> callback);
    }
}
