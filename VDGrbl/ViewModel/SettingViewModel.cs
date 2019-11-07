using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using VDGrbl.Service;
using System.Collections;
using System.Collections.ObjectModel;
using VDGrbl.Model;
using System.Collections.Generic;

namespace VDGrbl.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly ISettingService _settingService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string _groupBoxSettingTitle = string.Empty;
        private ObservableCollection<SettingItem> _settingCollection = new ObservableCollection<SettingItem>();

        #region RelayCommand
        public RelayCommand RefreshSettingCommand { get; private set; }
        private void SettingRelayCommands()
        {
            RefreshSettingCommand = new RelayCommand(RefreshSetting, CanExecuteRefreshSettingCommand);
            logger.Info("SettingViewModel|SettingRelayCommands initialised");

        }
        private void SettingMessengers()
        {
            MessengerInstance.Register<PropertyChangedMessage<ObservableCollection<SettingItem>>>(this, SearchMainViewModelChanged);
            logger.Info("SettingViewModel|SettingMessengers initialised");
        }
        #endregion

        #region Messenger
        /// <summary>
        /// Used to communicate between ViewModels: MainViewModel
        /// </summary>
        public void SendSettingMessage()
        {
            MessengerInstance.Send<NotificationMessage>(new NotificationMessage("GetSetting"));
            logger.Info("SettingViewModel|Notification GetSetting sent");
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
                logger.Info("SettingViewModel|SettingCollection updated");
            }
        }
        #endregion

        #region Constructor
        public SettingViewModel(ISettingService settingService)
        {
            _settingService = settingService;
            if (_settingService != null)
            {
                _settingService.GetSetting(
                        (item, error) =>
                        {
                            if (error != null)
                            {
                                logger.Error("SettingViewModel|Exception setting raised: " + error);
                                return;
                            }
                            logger.Info("SettingViewModel|Load settings window");
                            GroupBoxSettingTitle = item.SettingHeader;
                        });
                SettingRelayCommands();
                SettingMessengers();
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Sends the Grbl '$$' command with messenger to Main class to get all particular $x=var settings of the machine
        /// </summary>
        public void RefreshSetting()
        {
            logger.Debug("SettingViewModel|Send notification");
            SendSettingMessage();
        }
        /// <summary>
        /// Allows/disallows refresh grbl settings' button.
        /// </summary>
        /// <returns></returns>
        public static bool CanExecuteRefreshSettingCommand()
        {
            return true;
        }

        private void SearchMainViewModelChanged(PropertyChangedMessage<ObservableCollection<SettingItem>> propertyDetails)
        {
            if (propertyDetails.PropertyName == nameof(SettingCollection))
            {
                SettingCollection=propertyDetails.NewValue;
                logger.Info("SettingViewModel|SearchMainViewModelChanged()");
            }
        }
        #endregion
    }
}
