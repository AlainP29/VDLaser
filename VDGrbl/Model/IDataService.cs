using System;

namespace VDGrbl.Model
{
    public interface IDataService
    {
        void GetSerialPortSetting(Action<SerialPortSettingItems, Exception> callback);
        void GetGCode(Action<GCodeModel, Exception> callback);
        void GetLaserImage(Action<LaserImageModel, Exception> callback);
        void GetGraphic(Action<GraphicModel, Exception> callback);
        void GetConsole(Action<ConsoleModel, Exception> callback);
        void GetJogging(Action<JoggingModel, Exception> callback);

    }
}
