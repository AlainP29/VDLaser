namespace VDLaser.Model
{
    public class JoggingModel
    {
        public string JoggingHeader { get; private set; }
        public string Feed { get; private set; }
        public string Step { get; private set; }
        public bool IsSelectedKeyboard { get; private set; }
        public bool IsselectedMetric { get; private set; }

        public JoggingModel(string joggingHeader)
        {
            JoggingHeader = joggingHeader;
        }
    }
}
