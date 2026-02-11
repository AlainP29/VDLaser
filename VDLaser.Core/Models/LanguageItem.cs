using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Core.Models
{
    public class LanguageItem 
    { 
        public string Code { get; set; } = ""; 
        public string DisplayName { get; set; } = "";
        public string FlagPath => $"/VDLaser;component/Resources/Assets/Flags/{Code}.png";
    }
}
