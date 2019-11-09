using System;
using VDLaser.Model;
using VDLaser.Service;

namespace VDLaser.Design
{
    public class DesignSettingService : ISettingService
    {
        public void GetSetting(Action<SettingItem, Exception> callback)
        {
            var item = new SettingItem("Settings [design]");
            callback?.Invoke(item, null);
        }
    }
}
