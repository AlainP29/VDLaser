using GalaSoft.MvvmLight;

namespace VDGrbl.Model
{
    /// <summary>
    /// Grbl model class.
    /// </summary>
    public class GrblModel:ObservableObject
    {

        #region Properties
        /// <summary>
        /// Title of the groupbox data console
        /// </summary>
        public string GrblConsoleHeader { get; private set; } = "Data console";

        /// <summary>
        /// Title of the groupbox Grbl setting
        /// </summary>
        public string GrblSettingHeader { get; private set; } = "Grbl setting";

        /// <summary>
        /// Title of the groupbox Grbl command
        /// </summary>
        public string GrblCommandHeader { get; private set; } = "Grbl command";

        /// <summary>
        /// Get the line received
        /// </summary>
        public string RXData { get; private set; }
        
        /// <summary>
        /// Get the line transmitted
        /// </summary>
        public string TXData { get; private set; }

        /// <summary>
        /// return the list of codes of the Grbl settings
        /// </summary>
        public string SettingCode { get; private set; }

        /// <summary>
        /// Get the list of values of Grbl settings
        /// </summary>
        public string SettingValue { get; private set; }

        /// <summary>
        /// Return the list of description of the Grbl settings
        /// </summary>
        public string SettingDescription { get; private set; }
        #endregion  

        #region Constructor
        public GrblModel(string grblHeader)
        {
            GrblConsoleHeader += grblHeader;
            GrblSettingHeader += grblHeader;
            GrblCommandHeader += grblHeader;
        }
        /// <summary>
        /// Initialize a new instance of GrblModel with 2 parameters for Console data. 
        /// </summary>
        /// <param name="txDataInit"></param>
        /// <param name="rxData"></param>
        public GrblModel(string txData, string rxData)
        {
            TXData = txData;
            TXData = rxData;
        }

        /// <summary>
        /// Initialize a new instance of GrblModel with 3 parameters for Grbl settings. 
        /// </summary>
        /// <param name="settingCode"></param>
        /// <param name="settingValue"></param>
        /// <param name="settingDescription"></param>
        public GrblModel(string settingCode, string settingValue, string settingDescription)
        {
            SettingCode = settingCode;
            SettingValue = settingValue;
            SettingDescription = settingDescription;
        }
        #endregion
    }
}
