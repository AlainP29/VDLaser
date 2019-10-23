using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using VDGrbl.Model;
using NLog;
using System.Collections.ObjectModel;
using System.IO.Ports;

namespace VDGrbl.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private ObservableCollection<SettingItem> _settingCollection = new ObservableCollection<SettingItem>();
        private string _groupBoxSettingTitle = string.Empty;
        private bool _isRefresh = true;

        #region RelayCommand
        public RelayCommand SettingCommand { get; private set; }
        public RelayCommand RefreshSettingCommand { get; private set; }
        private void MyRelayCommands()
        {
            SettingCommand = new RelayCommand(GrblSetting, CanExecuteSettingCommand);
            RefreshSettingCommand = new RelayCommand(RefreshSettings, CanExecuteRefreshSettingCommand);
        }
        #endregion

        #region Property
        /// <summary>
        /// Get the GroupBoxSettingTitle property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public string GroupBoxSettingTitle
        {
            get
            {
                return _groupBoxSettingTitle;
            }
            set
            {
                Set(ref _groupBoxSettingTitle, value);
            }
        }
        /// <summary>
        /// Gets the IsRefresh property. if IsRefresh is true load settings is ok.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsRefresh
        {
            get
            {
                return _isRefresh;
            }
            set
            {

                Set(ref _isRefresh, value);
            }
        }
        /// <summary>
        /// Get the ListSettingModel property. ListSettingsModel is populated w/ Grbl settings data ('$$' command)
        /// Changes to that property's value raise the PropertyChanged event. 
        /// First get ListSettingModel then populate the observableCollection settingCollection for binding.
        /// </summary>
        public List<SettingItem> ListSetting { get; set; }

        /// <summary>
        /// Gets the SettingCollection property. SettingCollection is populated w/ data from ListSettingModel
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SettingItem> SettingCollection
        {
            get
            {
                return _settingCollection;
            }
            set
            {
                Set(ref _settingCollection, value);
            }
        }
        #endregion

        #region Constructor
        public SettingsViewModel(ISettingsService settingService)
        {
            _settingService = settingService;
            if (_settingService != null)
            {
                _settingService.GetSettings(
                        (item, error) =>
                        {
                            if (error != null)
                            {
                                logger.Error("SettingsViewModel|Exception settings raised: " + error);
                                return;
                            }
                            logger.Info("SettingsViewModel|Load settings window");
                            GroupBoxSettingTitle = item.SettingHeader;

    });
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Sends the Grbl '$$' command to get all particular $x=var settings of the machine
        /// </summary>
        public void RefreshSettings()
        {
            ListSetting.Clear();
            //WriteString("$$");
            IsRefresh = true;
        }
        /// <summary>
        /// Get Grbl settings of the machine
        /// </summary>
        public void GrblSetting()
        {
            SettingCollection = new ObservableCollection<SettingItem>(ListSetting);
            IsRefresh = false;
        }
        
        /// <summary>
        /// Allows/disallows Load grbl settings' button.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteSettingCommand()
        {
            return true;
        }
        /// <summary>
        /// Allows/disallows refresh grbl settings' button.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteRefreshSettingCommand()
        {
            return true;
        }
        #endregion
    }
}
