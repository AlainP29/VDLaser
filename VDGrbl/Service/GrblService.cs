using System;
using VDGrbl.Model;

namespace VDGrbl.Service
{
    public class GrblService:IGrblService
    {
        public void GetMachineState(Action<GrblItems, Exception> callback)
        {
            var item = new GrblItems("Machine state");
            callback?.Invoke(item, null);
        }
        public void GetControle(Action<GrblItems, Exception> callback)
        {
            var item = new GrblItems("Controle");
            callback?.Invoke(item, null);
        }
    }
}
