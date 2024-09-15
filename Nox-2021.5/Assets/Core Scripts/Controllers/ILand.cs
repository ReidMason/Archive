using UnityEngine;
using System.Collections;

using NoxCore.Placeables;
using NoxCore.Fittings.Sockets;

namespace NoxCore.Controllers
{
    public interface ILand
    {
        IHangar getHangar();
        void setHangar(IHangar hangar);
        GameObject getHangarGO();
        Structure getHangarStructure();
        void setHangarStructure(Structure hangarStructure);
    }
}
