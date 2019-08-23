using System.Collections.Generic;

namespace VDGrbl.Model
{
    /// <summary>
    /// G-Code class model: add GCodeFileName, GCodeLine and GCodeFileList properties.
    /// </summary>
    public class GCodeModel
    {
        public string GCodeHeader { get; private set; }

        public GCodeModel(string gcodeFileHeader)
        {
            GCodeHeader = gcodeFileHeader;
        }
    }
}


