using System;

namespace VDGrbl.Model
{
    public interface IDataService
    {
        void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback);
        void GetGrbl(Action<GrblModel, Exception> callback);
        void GetGCode(Action<GCodeModel, Exception> callback);
        void GetDataField(Action<DataFieldModel, Exception> callback);
        void GetGrblSettings(Action<GrblSettingsModel, Exception> callback);
        void GetLaserImage(Action<LaserImageModel, Exception> callback);
        void GetGraphic(Action<GraphicModel, Exception> callback);
    }
}
