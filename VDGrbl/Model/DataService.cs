using System;

namespace VDGrbl.Model
{
    public class DataService : IDataService
    {
        public void GetData(Action<SerialPortSettingsModel, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new SerialPortSettingsModel("Port settings");
            callback(item, null);
        }

    }
}