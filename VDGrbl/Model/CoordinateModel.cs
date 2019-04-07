namespace VDGrbl.Model
{
    public class CoordinateModel
    {
        public string CoordinateHeader { get; private set; }
        public string X { get; private set; }
        public string Y { get; private set; }

        public CoordinateModel(string coordinateHeader)
        {
            CoordinateHeader = coordinateHeader;
        }

        public CoordinateModel(string x, string y)
        {
            X = x;
            Y = y;
        }
    }
}
