using System.Collections.Generic;

namespace VDGrbl.Model
{
    /// <summary>
    /// G-Code class model
    /// </summary>
    public class GCodeModel
    {
        public string GCodeHeader { get; private set; }
        public string GCodeFileName { get; private set; }
        public string GCodeLine { get; private set; }
        public List<string> GCodeFileList { get; private set; }

        public GCodeModel(string gcodeFileHeader)
        {
            GCodeHeader = gcodeFileHeader;
        }
    }
}


