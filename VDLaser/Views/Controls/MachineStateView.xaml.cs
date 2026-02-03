using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using VDLaser.ViewModels.Controls;

namespace VDLaser.Views.Controls
{
    public partial class MachineStateView : UserControl
    {
        public MachineStateView()
        {
            InitializeComponent();
            var serviceProvider = (App.Current as App)?.ServiceProvider;
            if (serviceProvider != null)
            {
                DataContext = serviceProvider.GetRequiredService<MachineStateViewModel>();
            }
        }
    }
}
