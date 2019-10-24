using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDGrbl.Model
{
    public interface IGrblService
    {
        void GetMachineState(Action<GrblItems, Exception> callback);
        void GetControle(Action<GrblItems, Exception> callback);
    }
}
