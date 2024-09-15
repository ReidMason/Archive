using UnityEngine;
using System.Collections;

using NoxCore.Fittings.Modules;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.Controllers
{
    public interface IFormationFly
    {
        Vector2 getFormationOffset();
        void setFormationOffset(Vector2 formationoffset);
        Ship pickNewLeader();
        void setAsLeader();
        (Structure primaryTargetStructure, Module primaryTargetSystem) getPrimaryTarget();
    }
}
