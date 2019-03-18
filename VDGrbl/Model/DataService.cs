using System;

namespace VDGrbl.Model
{
    public class DataService : IDataService
    {
        public void GetPortSettings(Action<SerialPortSettingsModel, Exception> callback)
        {
            // Use this to connect to the actual data service

            var itemPortSettings = new SerialPortSettingsModel("Port settings");
            callback(itemPortSettings, null);
        }

        public void GetGrblData(Action<GrblModel, Exception> callback)
        {
            var itemGrbl = new GrblModel("Data send","Data received");
            callback(itemGrbl, null);
        }
    }
}