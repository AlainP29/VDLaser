using System;
using VDGrbl.Model;

namespace VDGrbl.Design
{
    public class DesignDataService : IDataService
    {
        /*public void GetData(Action<SerialPortSettingsModel, Exception> callback)
        {
            // Use this to create design time data

            var item = new SerialPortSettingsModel(" [design]");
            callback(item, null);
        }*/

        public void GetGrbl(Action<GrblModel, Exception> callback)
        {
            var item = new GrblModel("[design]");
            callback(item, null);
        }

        public void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback)
        {
            var item = new SerialPortSettingModel("Port settings [design]");
            callback(item, null);
        }

        public void GetGCode(Action<GCodeModel, Exception> callback)
        {
            var item = new GCodeModel("G-Code file [design]");
            callback(item, null);
        }

        public void GetMachineState(Action<DataFieldModel, Exception> callback)
        {
            var item = new DataFieldModel("Machine state [design]");
            callback(item, null);
        }

        public void GetLaserImage(Action<LaserImageModel, Exception> callback)
        {
            var item = new LaserImageModel("Laser image [design]");
            callback(item, null);
        }
    }
}