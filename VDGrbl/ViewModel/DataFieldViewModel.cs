using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using VDGrbl.Model;
using NLog;
using System.Windows.Media;

namespace VDGrbl.ViewModel
{
    public class DataFieldViewModel:ViewModelBase
    {
        #region Fields
        private readonly IDataService _dataService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string _groupBoxDataFieldTitle = string.Empty;
        private string _mposX = "0.000", _mposY = "0.000";
        private string _wposX = "0.000", _wposY = "0.000";
        private string _feed = "0", _speed = "0";
        private MachStatus _machineStatus = MachStatus.Idle;
        private SolidColorBrush _machineStatusColor = new SolidColorBrush(Colors.LightGray);
        private SolidColorBrush _laserColor = new SolidColorBrush(Colors.LightGray);

        /// <summary>
        /// Enumeration of the machine states:
        /// (V0.9) Idle, Run, Hold, Door, Home, Alarm, Check
        /// (V1.1) Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep
        /// </summary>
        public enum MachStatus { Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep };
        #endregion

        #region Properties
        /// <summary>
        /// Get the GroupBoxDataFieldTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string GroupBoxDataFieldTitle
        {
            get
            {
                return _groupBoxDataFieldTitle;
            }
            set
            {
                Set(ref _groupBoxDataFieldTitle, value);
            }
        }
        /// <summary>
        /// Get the MachineStatus property. This is the current state of the machine (Idle, Run, Hold, Alarm...)
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public MachStatus MachineStatus
        {
            get
            {
                return _machineStatus;
            }
            set
            {
                Set(ref _machineStatus, value);
            }
        }

        /// <summary>
        /// Get the MachineStatusColor property. The color change depending of the current state of the machin (Idle=Beige, Run=Light Green...)
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SolidColorBrush MachineStatusColor
        {
            get
            {
                return _machineStatusColor;
            }
            set
            {
                Set(ref _machineStatusColor, value);
            }
        }

        /// <summary>
        /// Get the MPosX property. MPosX is the X machine coordinate get w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string MPosX
        {
            get
            {
                return _mposX;
            }
            set
            {
                Set(ref _mposX, value);
            }
        }

        /// <summary>
        /// Get the MPosY property. MPosY is the Y machine coordinate get w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string MPosY
        {
            get
            {
                return _mposY;
            }
            set
            {
                Set(ref _mposY, value);
            }
        }

        /// <summary>
        /// Get the WPosX property. WPosX is the X work coordinate w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WPosX
        {
            get
            {
                return _wposX;
            }
            set
            {
                Set(ref _wposX, value);
            }
        }

        /// <summary>
        /// Get the WPosY property. WPosY is the Y work coordinate w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WPosY
        {
            get
            {
                return _wposY;
            }
            set
            {
                Set(ref _wposY, value);
            }
        }

        /// <summary>
        /// Get the Feed property. Feed is the real time feed of the machine get w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Feed
        {
            get
            {
                return _feed;
            }
            set
            {
                Set(ref _feed, value);
            }
        }

        /// <summary>
        /// Get the Speed property. Speed is the real time spindle speed or laser power of the machine get w/ '?' Grbl real-time command.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                Set(ref _speed, value);
            }
        }
        #endregion

        /// <summary>
        /// Initialize a new instance of the DataFieldViewModel class.
        /// </summary>
        public DataFieldViewModel(IDataService dataService)
        {
            logger.Info("Starting DataFieldViewModel");
            _dataService = dataService;
            _dataService.GetDataField(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                            logger.Error("DataFieldViewModel|Exception Coordinate raised: " + error);
                            return;
                        }
                        logger.Info("DataFieldViewModel|Load Coordinate window");
                        GroupBoxDataFieldTitle = item.DataFieldHeader;
                    });
        }


    }
}
