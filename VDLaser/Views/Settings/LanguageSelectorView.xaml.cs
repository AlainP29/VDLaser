using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VDLaser.ViewModels.Settings;

namespace VDLaser.Views.Settings
{
    /// <summary>
    /// It connects the ViewModel and exposes an event to the outside.
    /// </summary>
    public partial class LanguageSelectorView : UserControl 
    { 
        public LanguageSelectorViewModel ViewModel { get; } 
        public event Action<string>? LanguageChanged; 
        public LanguageSelectorView() 
        { 
            InitializeComponent(); 
            ViewModel = new LanguageSelectorViewModel(); 
            DataContext = ViewModel; 
            ViewModel.LanguageChanged += lang => LanguageChanged?.Invoke(lang); 
        } 
    }
}
