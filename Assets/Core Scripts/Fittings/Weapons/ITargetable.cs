using UnityEngine;

namespace NoxCore.Fittings.Weapons
{
    public interface ITargetable
    {
        (GameObject structure, GameObject system)? Target { get; set; }
    }
}