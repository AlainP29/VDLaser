using System;
using VDGrbl.Model;

namespace VDGrbl.Design
{
    public class DesignSettingService : ISettingService
    {
        public void GetSettings(Action<SettingItem, Exception> callback)
        {
            var item = new SettingItem("Settings [design]");
            callback?.Invoke(item, null);
        }
    }
}
