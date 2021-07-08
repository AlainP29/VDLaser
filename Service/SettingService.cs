using System;
using VDLaser.Model;

namespace VDLaser.Service
{
    public class SettingService : ISettingService
    {
        public void GetSetting(Action<SettingItems, Exception> callback)
        {
            var item = new SettingItems();
            callback?.Invoke(item, null);
        }
    }
}
