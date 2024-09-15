using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace Davin.Missions
{
    public class SimpleWarpIn : MonoBehaviour
    {
        public float delay;
        public Ship ship;

        public Vector2 warpPosition;
        public float warpRotation;

        void Awake()
        {
            StartCoroutine(delayedWarpIn());
        }

        IEnumerator delayedWarpIn()
        {
            yield return new WaitForSeconds(delay);

            if (ship != null)
            {
                ship.Call_WarpIn(this, new WarpEventArgs(ship.gameObject, SceneManager.GetActiveScene().name, null, warpPosition, warpRotation));
            }
        }
    }
}