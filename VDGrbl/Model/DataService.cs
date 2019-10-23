using System;

namespace VDGrbl.Model
{
    public class DataService : IDataService
    {
        public void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new SerialPortSettingModel("Port Settings");
            callback?.Invoke(item, null);
        }
        public void GetCommand(Action<CommandModel, Exception> callback)
        {
            var item = new CommandModel("Command");
            callback?.Invoke(item, null);
        }
        public void GetGCode(Action<GCodeModel, Exception> callback)
        {
            var item = new GCodeModel("G-Code file");
            callback?.Invoke(item, null);
        }
        public void GetDataField(Action<GrblItems, Exception> callback)
        {
            var item = new GrblItems("Machine state");
            callback?.Invoke(item, null);
        }
        public void GetInformation(Action<InformationModel, Exception> callback)
        {
            var item = new InformationModel("Infos");
            callback?.Invoke(item, null);
        }
        /*public void GetSettings(Action<SettingsItem, Exception> callback)
        {
            var item = new SettingsItem("Settings");
            callback?.Invoke(item, null);
        }*/
        public void GetLaserImage(Action<LaserImageModel, Exception> callback)
        {
            var item = new LaserImageModel("Laser image");
            callback?.Invoke(item, null);
        }
        public void GetGraphic(Action<GraphicModel, Exception> callback)
        {
            var item = new GraphicModel("Graphic");
            callback?.Invoke(item, null);
        }
        public void GetControl(Action<ControlModel, Exception> callback)
        {
            var item = new ControlModel("Control");
            callback?.Invoke(item, null);
        }
        public void GetConsole(Action<ConsoleModel, Exception> callback)
        {
            var item = new ConsoleModel("Console");
            callback?.Invoke(item, null);
        }

        public void GetJogging(Action<JoggingModel, Exception> callback)
        {
            var item=new JoggingModel("Jog");
            callback?.Invoke(item, null);
        }
    }
}