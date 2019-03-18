using GalaSoft.MvvmLight;

namespace VDGrbl.Model
{
    /// <summary>
    /// Grbl model class.
    /// </summary>
    public class GrblModel:ObservableObject
    {
        #region Fields
        private string _rxData;
        private string _txData;
        #endregion

        #region Properties
        /// <summary>
        /// Get the line received
        /// </summary>
        public string RXData
        {
            get
            {
                return _rxData;
            }
            set
            {
                Set(ref _rxData, value);
            }
        }

        /// <summary>
        /// Get the line transmitted
        /// </summary>
        public string TXData
        {
            get
            {
                return _txData;
            }
            set
            {
                Set(ref _txData, value);
            }
        }
        #endregion  

        #region Constructor
        /// <summary>
        /// Initialize the GrblModel. 
        /// </summary>
        /// <param name="txDataInit"></param>
        /// <param name="rxDataInit"></param>
        public GrblModel(string txDataInit, string rxDataInit)
        {
            _txData = txDataInit;
            _rxData = rxDataInit;
        }
        #endregion
    }
}
