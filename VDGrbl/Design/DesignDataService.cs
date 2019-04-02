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

        public void GetGrblSetting(Action<GrblSettingModel, Exception> callback)
        {
            var item = new GrblSettingModel("Grbl setting [design]");
            callback(item, null);
        }

        public void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback)
        {
            var item = new SerialPortSettingModel("Port setting [design]");
            callback(item, null);
        }

        public void GetGCodeFile(Action<GCodeFileModel, Exception> callback)
        {
            var item = new GCodeFileModel("G-Code File [design]");
            callback(item, null);
        }
    }
}