using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Managers;
using NoxCore.Data.Placeables;

namespace NoxCore.Placeables
{
    public class MovingStructure : Structure
    {
        protected IEnumerator lerper;

        public float rotationRate;
        public Vector3? rotationAxis;
        public List<Transform> waypoints;
        protected int currentWaypoint;
        protected Transform currentMoveTarget;
        public float moveSpeed;
        public bool loop;

        public override void init(NoxObjectData noxObjectData = null)
        {
            if (noxObjectData == null)
            {
                StructureData = noxObject2DData as StructureData;

                base.init(StructureData);
            }
            else
            {
                StructureData = noxObjectData as StructureData;
                base.init(noxObjectData);
            }

            if (waypoints.Count > 0)
            {
                lerper = MultipleLerp(waypoints, moveSpeed);
                StartCoroutine(lerper);
            }

            if (rotationAxis == null)
            {
                rotationAxis = Vector3.up;
            }
        }

        protected IEnumerator MultipleLerp(List<Transform> waypoints, float speed)
        {
            do
            {
                Vector3 startPos = waypoints[0].position;

                for (int i = 0; i < waypoints.Count; i++)
                {
                    float timer = 0f;

                    while (timer <= 1f)
                    {
                        while (GameManager.Instance.getSuspended() == true)
                        {
                            yield return new WaitForEndOfFrame();
                        }

                        timer += Time.deltaTime * speed;

                        Vector3 newPos;

                        if (i + 1 == waypoints.Count)
                        {
                            newPos = Vector3.Lerp(startPos, waypoints[0].position, timer);
                        }
                        else
                        {
                            newPos = Vector3.Lerp(startPos, waypoints[i + 1].position, timer);
                        }

                        transform.position = newPos;
                        yield return new WaitForEndOfFrame();
                    }

                    if (i + 1 == waypoints.Count)
                    {
                        transform.position = waypoints[0].position;
                        startPos = waypoints[0].position;
                    }
                    else
                    {
                        transform.position = waypoints[i + 1].position;
                        startPos = waypoints[i + 1].position;
                    }
                }
            }
            while (loop);

            yield return false;
        }

        public virtual void FixedUpdate()
        {
            if (GameManager.Instance.getSuspended() == false)
            {
                if (Destroyed == false)
                {
                    // rotate
                    transform.Rotate(rotationAxis.GetValueOrDefault(), rotationRate);
                }
                else
                {
                    StopCoroutine(lerper);
                }
            }
        }
    }
}
