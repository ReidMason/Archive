using UnityEngine;
using System.Collections.Generic;

using NoxCore.Fittings.Devices;

namespace Davin.Fittings.Devices
{
    public interface IAsteroidScanner : IScanner
    {
        List<GameObject> AsteroidsInRange { get; set; }
    }
}