
namespace VDLaser.Core.Models
{
    public partial class JoggingItems
    {
        public double Step { get; private set; } = 1;  // Pas en mm

        public double Feed { get; private set; } = 100;  // mm/min

        public bool IsSelectedKeyboard { get; private set; } = true;

        public bool IsSelectedMetric { get; private set; } = true;

        public bool IsSelectedImperial { get; private set; } = true;

        public string UpKey { get; private set; } = "Up";

        public string DownKey { get; private set; } = "Down";

        public string LeftKey { get; private set; } = "Left";

        public string RightKey { get; private set; } = "Right";


        // Ajoute directions (up/down/left/right)
        public JoggingItems()
        {

        }
    }
}
