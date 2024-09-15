using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoxCore.Fittings.Sockets
{
    public interface ISocket
    {
        void init();
        void reset();
        Transform Transform { get; }
        StructureSocketInfo getSocketInfo();
        bool install(GameObject go);
        void update();
    }
}