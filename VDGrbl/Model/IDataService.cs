using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDGrbl.Model
{
    public interface IDataService
    {
        void GetSerialPortSetting(Action<SerialPortSettingModel, Exception> callback);
        void GetGrblSetting(Action<GrblSettingModel, Exception> callback);
        void GetGCodeFile(Action<GCodeFileModel, Exception> callback);
    }
}
