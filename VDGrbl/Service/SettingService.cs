using System;
using VDGrbl.Model;

namespace VDGrbl.Service
{
    public class SettingService : ISettingService
    {
        public void GetSetting(Action<SettingItem, Exception> callback)
        {
            var item = new SettingItem("Settings");
            callback?.Invoke(item, null);
        }
    }
}
