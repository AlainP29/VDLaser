using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Core.Grbl.Commands
{
    public enum GrblCommandResult
    {
        Ok,
        Error,
        Timeout,
        Cancelled
    }
}

