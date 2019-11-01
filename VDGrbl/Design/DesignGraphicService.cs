using System;
using VDGrbl.Model;
using VDGrbl.Service;

namespace VDGrbl.Design
{
    public class DesignGraphicService : IGraphicService
    {
        public void GetGraphic(Action<GraphicItems, Exception> callback)
        {
            var item = new GraphicItems("Graphic [design]");
            callback?.Invoke(item, null);
        }
    }
}
