using System;
using VDGrbl.Model;

namespace VDGrbl.Design
{
    public class DesignDataService : IDataService
    {
        public void GetData(Action<SerialPortSettingsModel, Exception> callback)
        {
            // Use this to create design time data

            var item = new SerialPortSettingsModel("Port settings [design]");
            callback(item, null);
        }
    }
}