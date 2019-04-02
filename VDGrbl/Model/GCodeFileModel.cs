using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace VDGrbl.Model
{
    /// <summary>
    /// File model class.
    /// </summary>
    public class GCodeFileModel : ObservableObject
    {
        #region Properties
        /// <summary>
        /// Title of the groupbox G-code file
        /// </summary>
        public string GCodeFileHeader { get; private set; }

        /// <summary>
        /// The name of the G-code file
        /// </summary>
        public string GCodeFileName { get; private set; }

        /// <summary>
        /// List of G-code lines
        /// </summary>
        public List<string> GCodeFilelist { get; private set; }
        #endregion

        #region Constructor
        public GCodeFileModel(string gcodeFileHeader)
        {
            GCodeFileHeader = gcodeFileHeader;
        }
        #endregion
    }
}
