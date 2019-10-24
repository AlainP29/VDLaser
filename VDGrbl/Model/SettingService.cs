using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDGrbl.Model
{
    public class SettingService : ISettingService
    {
        public void GetSettings(Action<SettingItem, Exception> callback)
        {
            var item = new SettingItem("Settings");
            callback?.Invoke(item, null);
        }
    }
}
