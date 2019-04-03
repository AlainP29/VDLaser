namespace VDGrbl.Model
{
    public class CoordinateModel
    {
        /// <summary>
        /// Get the X property. The X position.
        /// </summary>
        public string X { get; private set; }

        /// <summary>
        /// Get the Y property. The Y position.
        /// </summary>
        public string Y { get; private set; }

        /// <summary>
        /// Title of the groupbox G-code file
        /// </summary>
        public string CoordinateHeader { get; private set; }

        public CoordinateModel(string coordinateHeader)
        {
            CoordinateHeader = coordinateHeader;
        }
    }
}
