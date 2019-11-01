using System;
using VDGrbl.Model;

namespace VDGrbl.Service
{
    public interface IGraphicService
    {
        void GetGraphic(Action<GraphicItems, Exception> callback);
    }
}
