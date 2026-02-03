using CommunityToolkit.Mvvm.ComponentModel;
using VDLaser.Core.Interfaces;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Controls
{
    public partial class LoggingSettingsViewModel : ViewModelBase
    {
        #region Properties
        private readonly ILogService _logService;
        [ObservableProperty]
        private LogProfile _selectedProfile;
        public string CurrentProfileDisplay
            => $"Logs : {SelectedProfile}";
        public IEnumerable<LogProfile> LogProfileValues => Enum.GetValues(typeof(LogProfile)).Cast<LogProfile>();
        #endregion

        public LoggingSettingsViewModel(ILogService logService)
        {
            _logService = logService;

            SelectedProfile = LogProfile.Normal;
            _logService.SetProfile(SelectedProfile);
        }

        #region Events
        partial void OnSelectedProfileChanged(LogProfile value)
        {
            _logService.SetProfile(value);
            OnPropertyChanged(nameof(CurrentProfileDisplay));
        }
        #endregion
    }
}
