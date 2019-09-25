using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDGrbl.Model
{
    public interface IDataService
    {
        void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback);
        void GetGrbl(Action<GrblModel, Exception> callback);
        void GetGCode(Action<GCodeModel, Exception> callback);
        void GetMachineState(Action<MachineStateModel, Exception> callback);
        void GetImage(Action<ImageModel, Exception> callback);
    }
}
