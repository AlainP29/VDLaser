using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VDLaser.Core.Models;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Settings
{
    public partial class LanguageSelectorViewModel : ViewModelBase
    {
        #region Properties
        public ObservableCollection<LanguageItem> Languages { get; } = 
            new() 
            { new LanguageItem { Code = "en", DisplayName = "English" }, 
                new LanguageItem { Code = "fr", DisplayName = "Français" } 
            };

        [ObservableProperty]
        private LanguageItem _selectedLanguage;
        #endregion

        public LanguageSelectorViewModel() 
        { 
            SelectedLanguage = Languages.First(l => l.Code == "en"); 
        }

        #region Events
        public event Action<string>? LanguageChanged;
        partial void OnSelectedLanguageChanged(LanguageItem value)
        {
            LanguageChanged?.Invoke(value.Code);
        }
        #endregion
    }
}
