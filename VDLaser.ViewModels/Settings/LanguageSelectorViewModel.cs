using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using VDLaser.Core.Models;
using VDLaser.ViewModels.Base;

namespace VDLaser.ViewModels.Settings
{
    public partial class LanguageSelectorViewModel : ViewModelBase
    {
        public ObservableCollection<LanguageItem> Languages { get; } = 
            new() 
            { new LanguageItem { Code = "en", DisplayName = "English" }, 
                new LanguageItem { Code = "fr", DisplayName = "Français" } 
            };

        [ObservableProperty]
        private LanguageItem _selectedLanguage;

        public event Action<string>? LanguageChanged;
        public LanguageSelectorViewModel() 
        { 
            SelectedLanguage = Languages.First(l => l.Code == "en"); 
        }
        partial void OnSelectedLanguageChanged(LanguageItem value)
        {
            LanguageChanged?.Invoke(value.Code);
        }
    }
}
