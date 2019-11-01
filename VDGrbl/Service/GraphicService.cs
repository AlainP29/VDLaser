using System;
using VDGrbl.Model;

namespace VDGrbl.Service
{
    class GraphicService : IGraphicService
    {
        public void GetGraphic(Action<GraphicItems, Exception> callback)
        {
            var item = new GraphicItems("Graphic");
            callback?.Invoke(item, null);
        }
    }
}
