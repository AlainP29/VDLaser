using System;

namespace VDGrbl.Model
{
    public class DataService : IDataService
    {
        public void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new SerialPortSettingModel("Port Setting");
            callback(item, null);
        }

        public void GetGrbl(Action<GrblModel, Exception> callback)
        {
            var item = new GrblModel("");
            callback(item, null);
        }

        public void GetGCode(Action<GCodeModel, Exception> callback)
        {
            var item = new GCodeModel("G-Code File");
            callback(item, null);
        }

        public void GetCoordinate(Action<CoordinateModel, Exception> callback)
        {
            var item = new CoordinateModel("X-Y Coordinate");
            callback(item, null);
        }
    }
}