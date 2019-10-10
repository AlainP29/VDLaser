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
        void GetMachineState(Action<DataFieldModel, Exception> callback);
        void GetLaserImage(Action<LaserImageModel, Exception> callback);
        void GetGraphic(Action<GraphicModel, Exception> callback);
    }
}
