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

        public void GetGrbl(Action<GrblModel, Exception> callback)
        {
            var item = new GrblModel("Grbl [design]");
            callback(item, null);
        }

        public void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback)
        {
            var item = new SerialPortSettingModel("Port setting [design]");
            callback(item, null);
        }

        public void GetGCode(Action<GCodeModel, Exception> callback)
        {
            var item = new GCodeModel("G-Code File [design]");
            callback(item, null);
        }

        public void GetCoordinate(Action<CoordinateModel, Exception> callback)
        {
            var item = new CoordinateModel("X-Y Coordinate [design]");
            callback(item, null);
        }

        public void GetImage(Action<ImageModel, Exception> callback)
        {
            var item = new ImageModel("Image [design]");
            callback(item, null);
        }
    }
}