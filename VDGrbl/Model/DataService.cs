﻿using System;

namespace VDGrbl.Model
{
    public class DataService : IDataService
    {
        public void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new SerialPortSettingModel("Port Settings");
            callback(item, null);
        }

        public void GetGrbl(Action<GrblModel, Exception> callback)
        {
            var item = new GrblModel(" Grbl");
            callback(item, null);
        }

        public void GetGCode(Action<GCodeModel, Exception> callback)
        {
            var item = new GCodeModel("G-Code File");
            callback(item, null);
        }

        public void GetMachineState(Action<MachineStateModel, Exception> callback)
        {
            var item = new MachineStateModel("Machine state");
            callback(item, null);
        }

        public void GetImage(Action<ImageModel, Exception> callback)
        {
            var item = new ImageModel("Image");
            callback(item, null);
        }
    }
}