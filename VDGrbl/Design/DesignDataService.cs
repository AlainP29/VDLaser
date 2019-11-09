using System;
using VDLaser.Model;
using VDLaser.Service;

namespace VDLaser.Design
{
    public class DesignDataService : IDataService
    {
        public void GetSerialPortSetting(Action<SerialPortSettingItems, Exception> callback)
        {
            var item = new SerialPortSettingItems("Port settings [design]");
            callback?.Invoke(item, null);
        }
        public void GetGCode(Action<GCodeModel, Exception> callback)
        {
            var item = new GCodeModel("G-Code file [design]");
            callback?.Invoke(item, null);
        }
        public void GetGraphic(Action<GraphicItems, Exception> callback)
        {
            var item = new GraphicItems("Graphic [design]");
            callback?.Invoke(item, null);
        }
        public void GetConsole(Action<ConsoleModel, Exception> callback)
        {
            var item = new ConsoleModel("Console [design]");
            callback?.Invoke(item, null);
        }
        public void GetJogging(Action<JoggingModel, Exception> callback)
        {
            var item = new JoggingModel("Jog [design]");
            callback?.Invoke(item, null);
        }
    }
}