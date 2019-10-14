using System;
using VDGrbl.Model;

namespace VDGrbl.Design
{
    public class DesignDataService : IDataService
    {
        public void GetGrbl(Action<GrblModel, Exception> callback)
        {
            var item = new GrblModel("[design]");
            callback?.Invoke(item, null);
        }
        public void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback)
        {
            var item = new SerialPortSettingModel("Port settings [design]");
            callback?.Invoke(item, null);
        }
        public void GetGCode(Action<GCodeModel, Exception> callback)
        {
            var item = new GCodeModel("G-Code file [design]");
            callback?.Invoke(item, null);
        }
        public void GetDataField(Action<DataFieldModel, Exception> callback)
        {
            var item = new DataFieldModel("Machine state [design]");
            callback?.Invoke(item, null);
        }
        public void GetGrblSettings(Action<GrblSettingsModel, Exception> callback)
        {
            var item = new GrblSettingsModel("Settings [design]");
            callback?.Invoke(item, null);
        }
        public void GetLaserImage(Action<LaserImageModel, Exception> callback)
        {
            var item = new LaserImageModel("Laser image [design]");
            callback?.Invoke(item, null);
        }
        public void GetGraphic(Action<GraphicModel, Exception> callback)
        {
            var item = new GraphicModel("Graphic [design]");
            callback?.Invoke(item, null);
        }
    }
}