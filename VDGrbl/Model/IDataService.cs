using System;

namespace VDGrbl.Model
{
    public interface IDataService
    {
        void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback);
        void GetCommand(Action<CommandModel, Exception> callback);
        void GetGCode(Action<GCodeModel, Exception> callback);
        void GetDataField(Action<GrblItems, Exception> callback);
        void GetInformation(Action<InformationModel, Exception> callback);
        //void GetSettings(Action<SettingsItem, Exception> callback);
        void GetLaserImage(Action<LaserImageModel, Exception> callback);
        void GetGraphic(Action<GraphicModel, Exception> callback);
        void GetControl(Action<ControlModel, Exception> callback);
        void GetConsole(Action<ConsoleModel, Exception> callback);
        void GetJogging(Action<JoggingModel, Exception> callback);

    }
}
