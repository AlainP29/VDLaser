using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDGrbl.Model
{
    public interface IDataService
    {
        void GetPortSettings(Action<SerialPortSettingsModel, Exception> callback);
        void GetGrblData(Action<GrblModel, Exception> callback);
    }
}
