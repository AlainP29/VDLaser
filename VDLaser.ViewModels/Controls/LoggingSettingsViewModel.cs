using CommunityToolkit.Mvvm.ComponentModel;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    /// <summary>
    /// Manages the application's logging profile, determining the verbosity 
    /// and filtering logic for system diagnostics.
    /// </summary>
    public partial class LoggingSettingsViewModel : ViewModelBase
    {
        #region Fields & Services
        private readonly ILogService _log;
        #endregion

        #region Properties
        [ObservableProperty]
        private LogProfile _selectedProfile;
        public string CurrentProfileDisplay
            => $"Logs : {SelectedProfile}";
        public IEnumerable<LogProfile> LogProfileValues => Enum.GetValues(typeof(LogProfile)).Cast<LogProfile>();
        #endregion

        public LoggingSettingsViewModel(ILogService log)
        {
            _log = log;

            SelectedProfile = LogProfile.Normal;
            _log.SetProfile(SelectedProfile);

            LogContextual(_log, "Initialized", $"Logging initialized with profile: {SelectedProfile}");
        }

        #region Events
        partial void OnSelectedProfileChanged(LogProfile value)
        {
            _log.SetProfile(value);
            OnPropertyChanged(nameof(CurrentProfileDisplay));
            LogContextual(_log, "ProfileChanged", $"Logging profile changed to: {value}");
        }
        #endregion
    }
}
