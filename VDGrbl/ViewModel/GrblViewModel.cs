using GalaSoft.MvvmLight;
using NLog;
using VDGrbl.Model;

namespace VDGrbl.ViewModel
{
    public class GrblViewModel : ViewModelBase
    {
        private readonly IGrblService _grblService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string _groupBoxMachineStateTitle = string.Empty;
        private string _groupBoxControleTitle = string.Empty;

        #region Property
        /// <summary>
        /// Get the GroupBoxGrblTitle property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public string GroupBoxMachineStateTitle
        {
            get
            {
                return _groupBoxMachineStateTitle;
            }
            set
            {
                Set(ref _groupBoxMachineStateTitle, value);
            }
        }
        /// <summary>
        /// Get the GroupBoxGrblTitle property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public string GroupBoxControleTitle
        {
            get
            {
                return _groupBoxControleTitle;
            }
            set
            {
                Set(ref _groupBoxControleTitle, value);
            }
        }
        #endregion

        #region Constructor
        public GrblViewModel(IGrblService grblService)
        {
            _grblService = grblService;
            if (_grblService != null)
            {
                _grblService.GetMachineState(
                        (item, error) =>
                        {
                            if (error != null)
                            {
                                logger.Error("GrblViewModel|Exception setting raised: " + error);
                                return;
                            }
                            logger.Info("GrblViewModel|Load machine state window");
                            GroupBoxMachineStateTitle = item.MachineStateHeader;
                        });

                _grblService.GetControle(
                        (item, error) =>
                        {
                            if (error != null)
                            {
                                logger.Error("GrblViewModel|Exception controle raised: " + error);
                                return;
                            }
                            logger.Info("GrblViewModel|Load controle window");
                            GroupBoxControleTitle = item.ControleHeader;
                        });
            }
        }
        #endregion
    }
}
