using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VDLaser.Model;
using VDLaser.Service;


namespace VDLaser.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly ISettingService _settingService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string _groupBoxSettingTitle = string.Empty;
        private ObservableCollection<SettingItem> _settingCollection = new ObservableCollection<SettingItem>();
        private List<SettingItem> _listSetting = new List<SettingItem>();

        #region RelayCommand & Messengers
        public RelayCommand RefreshSettingCommand { get; private set; }
        public RelayCommand Test { get; set; }
        private void SettingRelayCommands()
        {
            RefreshSettingCommand = new RelayCommand(RefreshSetting, CanExecuteRefreshSettingCommand);
            Test = new RelayCommand(GetTest, CanExecuteTest);
            logger.Info("SettingViewModel|SettingRelayCommands initialised");

        }
        /// <summary>
        /// Register for change in MainViewModel/setting collection property
        /// </summary>
        private void SettingMessengers()
        {
            //MessengerInstance.Register<PropertyChangedMessage<ObservableCollection<SettingItem>>>(this, SearchSettingCollectionMainViewModelChanged);
            MessengerInstance.Register<PropertyChangedMessage<List<SettingItem>>>(this, SearchListSettingMainViewModelChanged);
            logger.Info("SettingViewModel|SettingMessengers initialised");
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
        /// <summary>
        /// Gets the ListSetting property. SettingCollection is populated w/ data from ListSettingModel
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<SettingItem> ListSetting
        {
            get
            {
                return _listSetting;
            }
            set
            {
                Set(ref _listSetting, value);
                logger.Info("SettingViewModel|ListSetting updated");
                SettingCollection = new ObservableCollection<SettingItem>(ListSetting);
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
        /// Used to communicate between ViewModels: MainViewModel
        /// </summary>
        public void SendSettingMessage()
        {
            MessengerInstance.Send<NotificationMessage>(new NotificationMessage("GetSetting"));
            logger.Info("SettingViewModel|Notification GetSetting sent");
        }
        /// <summary>
        /// Sends the Grbl '$$' command with messenger to Main class to get all particular $x=var settings of the machine
        /// </summary>
        public void RefreshSetting()
        {
            if (SettingCollection.Count > 0)
            {
                SettingCollection.Clear();
            }
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
        /// <summary>
        /// Get new value of MainViewModel list setting change
        /// </summary>
        /// <param name="propertyDetails"></param>
        private void SearchListSettingMainViewModelChanged(PropertyChangedMessage<List<SettingItem>> propertyDetails)
        {
            if (propertyDetails.PropertyName == nameof(ListSetting))
            {
                ListSetting = propertyDetails.NewValue;
                logger.Info("SettingViewModel|SearchMainViewModelListSettingChanged()");
            }
        }
        /// <summary>
        /// For dvpt only
        /// </summary>
        private void GetTest()
        {
            
        }
        private static bool CanExecuteTest()
        {
            return true;
        }
        #endregion
    }
}
