/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:VDGrbl.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
//using Microsoft.Practices.ServiceLocation;with nugget v1.3.0/5.3.0
using CommonServiceLocator;
using NLog;

using VDGrbl.Model;

namespace VDGrbl.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
                SimpleIoc.Default.Register<ISettingsService, Design.DesignSettingsService>();
            }
            else
            {
                SimpleIoc.Default.Register<IDataService, DataService>();
                SimpleIoc.Default.Register<ISettingsService, SettingService>();
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public SettingsViewModel Settings
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingsViewModel>();
            }
        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            NLog.LogManager.Shutdown(); // Flush and close down internal threads and timers
        }
    }
}