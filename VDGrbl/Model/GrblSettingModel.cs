using GalaSoft.MvvmLight;

namespace VDGrbl.Model
{
    /// <summary>
    /// Grbl settings model class.
    /// </summary>
    /*public class SettingModel : ObservableObject
    {
        #region Fields
        private string _settingCode;
        private string _settingValue;
        private string _settingDescription;
        #endregion

        #region Properties
        public string SettingHeader { get; private set; }

        /// <summary>
        /// return the list of codes of the Grbl settings
        /// </summary>
        public string SettingCode
        {
            get
            {
                return _settingCode;
            }
            set
            {
                Set(ref _settingCode, value);
            }
        }

        /// <summary>
        /// Get the list of values of Grbl settings
        /// </summary>
        public string SettingValue
        {
            get
            {
                return _settingValue;
            }
            set
            {
                Set(ref _settingValue, value);
            }
        }

        /// <summary>
        /// Return the list of description of the Grbl settings
        /// </summary>
        public string SettingDescription
        {
            get
            {
                return _settingDescription;
            }
            set
            {
                Set(ref _settingDescription, value);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initialize the SettingModel
        /// </summary>
        /// <param name="settingValue"></param>
        public SettingModel(string settingCode, string settingValue, string settingDescription)
        {
            _settingCode = settingCode;
            _settingValue = settingValue;
            _settingDescription = settingDescription;
        }
        #endregion
    }*/

    public class GrblSettingModel
    {
        #region public property
        /// <summary>
        /// Title of the groupbox setting
        /// </summary>
        public string SettingHeader { get; private set; }

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
        public GrblSettingModel()
        {
        }

        public GrblSettingModel(string settingHeader)
        {
            SettingHeader = settingHeader;
        }

        public GrblSettingModel(string settingCode, string settingValue, string settingDescription)
        {
            SettingCode = settingCode;
            SettingValue = settingValue;
            SettingDescription = settingDescription;
        }
        #endregion
    }
}
