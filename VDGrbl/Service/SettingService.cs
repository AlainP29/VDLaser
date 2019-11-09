using System;
using VDLaser.Model;

namespace VDLaser.Service
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
