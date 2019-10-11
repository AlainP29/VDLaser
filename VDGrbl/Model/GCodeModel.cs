using System.Collections.Generic;

namespace VDGrbl.Model
{
    /// <summary>
    /// G-Code class model: add GCodeFileName, GCodeLine and GCodeFileList properties.
    /// </summary>
    public class GCodeModel
    {
        public string GCodeHeader { get; private set; }
        public string GCodeLine { get; set; }

        public string X { get; private set; }

        public string Y { get; private set; }

        public string I { get; private set; }

        public string J { get; private set; }

        public string S { get; private set; }

        public string F { get; private set; }

        public string G { get; private set; }

        public string M { get; private set; }
        public int N { get; private set; }

        public GCodeModel(string gcodeFileHeader)
        {
            GCodeHeader = gcodeFileHeader;
        }

        public GCodeModel(int numero,string line)
        {
            GCodeLine = line;
            N = numero;
        }
    }
}


