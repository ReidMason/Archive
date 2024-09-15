using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace Davin.Missions
{
    public class SimpleWarpOut : MonoBehaviour
    {
        [Range(1.0f,600.0f)]
        public float initDelay;
        public float warpDelay;
        public Ship ship;

        public Vector2 warpAlignOffset;

        void Awake()
        {
            StartCoroutine(delayedWarpOut());
        }

        IEnumerator delayedWarpOut()
        {
            yield return new WaitForSeconds(initDelay);

            if (ship != null)
            {
                ship.Helm.destination = ship.Helm.Position + (warpAlignOffset * 1000);
            }

            yield return new WaitForSeconds(warpDelay);

            if (ship != null)
            {
                ship.Call_WarpOut(this, new WarpEventArgs(ship.gameObject, null, SceneManager.GetActiveScene().name, null, null));
            }
        }
    }
}