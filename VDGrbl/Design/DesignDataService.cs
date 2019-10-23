using System;
using VDGrbl.Model;

namespace VDGrbl.Design
{
    public class DesignDataService : IDataService
    {
        public void GetCommand(Action<CommandModel, Exception> callback)
        {
            var item = new CommandModel("Command [design]");
            callback?.Invoke(item, null);
        }
        public void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback)
        {
            var item = new SerialPortSettingModel("Port settings [design]");
            callback?.Invoke(item, null);
        }
        public void GetGCode(Action<GCodeModel, Exception> callback)
        {
            var item = new GCodeModel("G-Code file [design]");
            callback?.Invoke(item, null);
        }
        public void GetDataField(Action<GrblItems, Exception> callback)
        {
            var item = new GrblItems("Machine state [design]");
            callback?.Invoke(item, null);
        }
        public void GetInformation(Action<InformationModel, Exception> callback)
        {
            var item = new InformationModel("Infos [design]");
            callback?.Invoke(item, null);
        }
        /*public void GetSettings(Action<SettingsItem, Exception> callback)
        {
            var item = new SettingsItem("Settings [design]");
            callback?.Invoke(item, null);
        }*/
        public void GetLaserImage(Action<LaserImageModel, Exception> callback)
        {
            var item = new LaserImageModel("Laser image [design]");
            callback?.Invoke(item, null);
        }
        public void GetGraphic(Action<GraphicModel, Exception> callback)
        {
            var item = new GraphicModel("Graphic [design]");
            callback?.Invoke(item, null);
        }
        public void GetControl(Action<ControlModel, Exception> callback)
        {
            var item = new ControlModel("Control [design]");
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