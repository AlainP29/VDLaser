using System;
using VDLaser.Model;

namespace VDLaser.Service
{
    public interface IDataService
    {
        /// <summary>
        /// This method is used to test the DI
        /// </summary>
        /// <returns></returns>
        string GetCurrentDate();
        void GetSerialPortSetting(Action<SerialPortSettingItems, Exception> callback);
        void GetGCode(Action<GCodeItems, Exception> callback);
        void GetGraphic(Action<GraphicItems, Exception> callback);
        void GetConsole(Action<ConsoleItems, Exception> callback);
        void GetJogging(Action<JoggingItems, Exception> callback);
    }
}
