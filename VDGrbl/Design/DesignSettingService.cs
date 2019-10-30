using System;
using VDGrbl.Model;
using VDGrbl.Service;

namespace VDGrbl.Design
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
