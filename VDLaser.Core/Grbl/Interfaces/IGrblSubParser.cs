using VDLaser.Core.Grbl.Models;

namespace VDLaser.Core.Grbl.Interfaces
{
    public interface IGrblSubParser
    {
        string Name { get; }
        bool CanParse(string line);
        void Parse(string line, GrblState state);
        void Parse(string line, GrblInfo info);
    }

}
