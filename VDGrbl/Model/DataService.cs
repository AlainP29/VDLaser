using System;

namespace VDGrbl.Model
{
    public class DataService : IDataService
    {
        public void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new SerialPortSettingModel("Port setting");
            callback(item, null);
        }

        public void GetGrblSetting(Action<GrblSettingModel, Exception> callback)
        {
            var item = new GrblSettingModel("Grbl setting");
            callback(item, null);
        }

        public void GetGCodeFile(Action<GCodeFileModel, Exception> callback)
        {
            var item = new GCodeFileModel("G-Code File");
            callback(item, null);
        }
    }
}