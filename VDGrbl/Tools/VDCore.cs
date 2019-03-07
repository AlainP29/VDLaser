using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using VDGrbl.Model;

namespace VDGrbl.Tools
{
    /// <summary>
    /// Goal: Use the following model for mvvm first test:
    /// Model: GrblModel/RXLine string property only
    /// Tools: VDCore{instantiate serialport, datareceived_event to get line and create method w/ return object GrblModel}
    /// ViewModel: private VDcore, public GrblModel LastGrblModel{get;set;} and treatment of data in viewmodel
    /// Easier to start instead of doing threatment in Tools/model...
    /// </summary>
    public class VDCore
    {
        #region private fields
        private SerialPort _serialPort;
        private string[] _listPortNames;
        private string _selectedDevicePortName = string.Empty;
        private int[] _listBaudRates;
        private int _selectedBaudRate;
        #endregion

        #region Constructor
        public VDCore()
        {
            _serialPort = new SerialPort();
        }
        #endregion

    }
}
