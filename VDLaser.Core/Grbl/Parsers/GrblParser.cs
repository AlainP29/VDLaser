using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;

namespace VDLaser.Core.Grbl.Parsers
{
    public class GrblParser
    {
        private readonly List<IGrblSubParser> _parsers;
        public GrblState State { get; private set; } = new();

        public GrblParser()
        {
            _parsers = new List<IGrblSubParser>
        {
            new GrblResponseParser(),
            new GrblInfoParser(),
            new GrblStateParser(),
            new GrblSettingsParser()
        };
        }

        public void Parse(string line)
        {
            foreach (var parser in _parsers)
            {
                if (parser.CanParse(line))
                {
                    parser.Parse(line, State);
                    return;
                }
            }
        }
    }

}
