using System;
using VDLaser.Model;

namespace VDLaser.Service
{
    public interface IDataService
    {
        void GetSerialPortSetting(Action<SerialPortSettingItems, Exception> callback);
        void GetGCode(Action<GCodeModel, Exception> callback);
        void GetGraphic(Action<GraphicItems, Exception> callback);
        void GetConsole(Action<ConsoleModel, Exception> callback);
        void GetJogging(Action<JoggingModel, Exception> callback);

    }
}
