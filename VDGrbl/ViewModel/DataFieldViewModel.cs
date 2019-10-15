using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using VDGrbl.Model;
using NLog;

namespace VDGrbl.ViewModel
{
    public class DataFieldViewModel:ViewModelBase
    {
        private readonly IDataService _dataService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string _groupBoxDataFieldTitle = string.Empty;

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
