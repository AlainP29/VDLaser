using Microsoft.Extensions.DependencyInjection;

namespace VDLaser.ViewModel
{
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel
        => App.ServiceProvider.GetRequiredService<MainViewModel>();

        public GraphicViewModel GraphicViewModel
        => App.ServiceProvider.GetRequiredService<GraphicViewModel>();

        public GrblViewModel GrblViewModel
        => App.ServiceProvider.GetRequiredService<GrblViewModel>();

        public SettingViewModel SettingViewModel
        => App.ServiceProvider.GetRequiredService<SettingViewModel>();

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            NLog.LogManager.Shutdown(); // Flush and close down internal threads and timers
        }

    }
}
