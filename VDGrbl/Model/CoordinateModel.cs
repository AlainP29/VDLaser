namespace VDGrbl.Model
{
    /// <summary>
    /// Coordinate model: add X, Y properties and constructor?
    /// </summary>
    public class CoordinateModel
    {
        public string CoordinateHeader { get; private set; }

        public CoordinateModel(string coordinateHeader)
        {
            CoordinateHeader = coordinateHeader;
        }
    }
}
