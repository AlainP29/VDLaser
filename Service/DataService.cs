using System;
using System.Collections.Generic;
using System.Text;
using VDLaser.Model;

namespace VDLaser.Service
{
    public class DataService : IDataService
    {
        public string GetCurrentDate()
        {
            return DateTime.Now.ToLongDateString(); 
        }

        public void GetSerialPortSetting(Action<SerialPortSettingItems, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new SerialPortSettingItems();
            callback?.Invoke(item, null);
        }
        public void GetGCode(Action<GCodeItems, Exception> callback)
        {
            var item = new GCodeItems();
            callback?.Invoke(item, null);
        }
        public void GetGraphic(Action<GraphicItems, Exception> callback)
        {
            var item = new GraphicItems();
            callback?.Invoke(item, null);
        }
        public void GetConsole(Action<ConsoleItems, Exception> callback)
        {
            var item = new ConsoleItems();
            callback?.Invoke(item, null);
        }
        public void GetJogging(Action<JoggingItems, Exception> callback)
        {
            var item = new JoggingItems();
            callback?.Invoke(item, null);
        }
    }
}
