using System;

namespace VDGrbl.Model
{
    public class DataService : IDataService
    {
        public void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new SerialPortSettingModel("Port Settings");
            callback?.Invoke(item, null);
        }
        public void GetGrbl(Action<GrblModel, Exception> callback)
        {
            var item = new GrblModel("");
            callback?.Invoke(item, null);
        }
        public void GetGCode(Action<GCodeModel, Exception> callback)
        {
            var item = new GCodeModel("G-Code file");
            callback?.Invoke(item, null);
        }
        public void GetDataField(Action<DataFieldModel, Exception> callback)
        {
            var item = new DataFieldModel("Machine state");
            callback?.Invoke(item, null);
        }
        public void GetGrblSettings(Action<GrblSettingsModel, Exception> callback)
        {
            var item = new GrblSettingsModel("Settings");
            callback?.Invoke(item, null);
        }
        public void GetLaserImage(Action<LaserImageModel, Exception> callback)
        {
            var item = new LaserImageModel("Laser image");
            callback?.Invoke(item, null);
        }
        public void GetGraphic(Action<GraphicModel, Exception> callback)
        {
            var item = new GraphicModel("Graphic");
            callback?.Invoke(item, null);
        }
    }
}