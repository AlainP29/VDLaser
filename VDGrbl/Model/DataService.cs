using System;

namespace VDGrbl.Model
{
    public class DataService : IDataService
    {
        public void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new SerialPortSettingModel("Port Settings");
            callback(item, null);
        }

        public void GetGrbl(Action<GrblModel, Exception> callback)
        {
            var item = new GrblModel("");
            callback(item, null);
        }

        public void GetGCode(Action<GCodeModel, Exception> callback)
        {
            var item = new GCodeModel("G-Code file");
            callback(item, null);
        }

        public void GetMachineState(Action<DataFieldModel, Exception> callback)
        {
            var item = new DataFieldModel("Machine state");
            callback(item, null);
        }

        public void GetLaserImage(Action<LaserImageModel, Exception> callback)
        {
            var item = new LaserImageModel("Laser image");
            callback(item, null);
        }
    }
}