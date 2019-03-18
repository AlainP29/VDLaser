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

        public void GetGrblData(Action<GrblModel, Exception> callback)
        {
            var item = new GrblModel("Data send [design]", "Data received [design]");
            callback(item, null);
        }

        public void GetPortSettings(Action<SerialPortSettingsModel, Exception> callback)
        {
            var item = new SerialPortSettingsModel("Port settings [design]");
            callback(item, null);
        }
    }
}