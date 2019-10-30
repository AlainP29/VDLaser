using System;
using VDGrbl.Model;

namespace VDGrbl.Service
{
    public interface IGrblService
    {
        void GetMachineState(Action<GrblItems, Exception> callback);
        void GetControle(Action<GrblItems, Exception> callback);
    }
}
