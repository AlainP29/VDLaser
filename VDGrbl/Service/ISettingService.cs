using System;
using VDLaser.Model;

namespace VDLaser.Service
{
    public interface ISettingService
    {
        void GetSetting(Action<SettingItem, Exception> callback);
    }
}
