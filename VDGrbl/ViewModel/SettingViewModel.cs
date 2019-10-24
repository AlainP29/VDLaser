using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using VDGrbl.Model;

namespace VDGrbl.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly ISettingService _settingService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string _groupBoxSettingTitle = string.Empty;

        #region RelayCommand
        public RelayCommand TestSettingCommand { get; private set; }
        private void SettingRelayCommands()
        {
            TestSettingCommand = new RelayCommand(TestSetting, CanExecuteTestSettingCommand);
        }
        #endregion

        #region Messenger
        public void SendSettingMessage()
        {
            MessengerInstance.Send<NotificationMessage>(new NotificationMessage("$$"));
            logger.Debug("SettingViewModel|Notification sent");
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
        #endregion

        #region Constructor
        public SettingViewModel(ISettingService settingService)
        {
            _settingService = settingService;
            if (_settingService != null)
            {
                _settingService.GetSettings(
                        (item, error) =>
                        {
                            if (error != null)
                            {
                                logger.Error("SettingViewModel|Exception setting raised: " + error);
                                return;
                            }
                            logger.Info("SettingsViewModel|Load setting window");
                            GroupBoxSettingTitle = item.SettingHeader;
                        });
                SettingRelayCommands();
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Sends the Grbl '$$' command to get all particular $x=var settings of the machine
        /// </summary>
        public void TestSetting()
        {
            logger.Debug("SettingViewModel|Send notification");
            SendSettingMessage();
        }
        /// <summary>
        /// Allows/disallows refresh grbl settings' button.
        /// </summary>
        /// <returns></returns>
        public bool CanExecuteTestSettingCommand()
        {
            return true;
        }
        #endregion
    }
}
