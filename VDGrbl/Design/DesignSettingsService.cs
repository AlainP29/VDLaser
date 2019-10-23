using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDGrbl.Model;

namespace VDGrbl.Design
{
    public class DesignSettingsService : ISettingsService
    {
        public void GetSettings(Action<SettingItem, Exception> callback)
        {
            var item = new SettingItem("Settings [design]");
            callback?.Invoke(item, null);
        }
    }
}
