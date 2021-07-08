using GalaSoft.MvvmLight;
using NLog;
using VDLaser.Model;
using VDLaser.Service;

namespace VDLaser.ViewModel
{
    public class GrblViewModel : ViewModelBase
    {
        private readonly IGrblService _grblService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
                        });
            }
        }
        #endregion
    }
}
