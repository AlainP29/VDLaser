using System.Collections.Generic;

namespace VDLaser.Model
{
    public class GCodeModel
    {
        public string GCodeHeader { get; private set; }
        public string GCodeLine { get; set; }
        public string X { get; private set; }
        public string Y { get; private set; }
        public string I { get; private set; }
        public string J { get; private set; }
        public string L { get; private set; }
        public string T { get; private set; }
        public string S { get; private set; }
        public string F { get; private set; }
        public string G { get; private set; }
        public string M { get; private set; }
        public int N { get; private set; }

        public GCodeModel()
        { }
        public GCodeModel(string gcodeFileHeader)
        {
            GCodeHeader = gcodeFileHeader;
        }

        public GCodeModel(int numero, string line)
        {
            GCodeLine = line;
            N = numero;
        }
    }
}


