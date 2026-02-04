using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VDLaser.Core.Interfaces;
using VDLaser.Core.Models;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Settings
{
    public partial class SoftwareSettingViewModel : ViewModelBase
    {
        private readonly ISettingService _settingService;

        [ObservableProperty]
        private SoftwareSettings _settings = new();

        [ObservableProperty]
        private bool _isLoading;

        public SoftwareSettingViewModel(ISettingService settingService)
        {
            _settingService = settingService;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            IsLoading = true;
            Settings = await _settingService.GetSettingsAsync();
            IsLoading = false;
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            await _settingService.SaveSettingsAsync(Settings);
        }
    }
}
