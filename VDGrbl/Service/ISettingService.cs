using System;
using VDGrbl.Model;

namespace VDGrbl.Service
{
    public interface ISettingService
    {
        void GetSetting(Action<SettingItem, Exception> callback);
    }
}
