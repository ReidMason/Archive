using UnityEngine;
using System.Collections.Generic;

using NoxCore.Data;
using NoxCore.Utilities;

using NoxCore.Fittings.Sockets;

namespace NoxCore.Helm
{
    public class NewAvoidBehaviour : SteeringBehaviour
    {
        [ShowOnly]
        protected List<IFeelerData> _feelers;
        public List<IFeelerData> Feelers { get; set; }

        protected LayerMask collidables;
        protected int maxFrameCounter;
        protected int frameCounter;
        protected float length, width, shipRadius;
        protected Vector2 combinedFeelerForce, prevForce;

        void Reset()
        {
            Label = "AVOID";
            SequenceID = 4;
            Weight = 1000;
        }

        void Awake()
        {
            Feelers = new List<IFeelerData>();

            length = transform.parent.GetComponent<PolygonCollider2D>().bounds.extents.y;
            width = transform.parent.GetComponent<PolygonCollider2D>().bounds.extents.x;
        }

        public override void Start()
        {
            base.Start();

            combinedFeelerForce = Vector2.zero;
            shipRadius = Mathf.Max(length, width);

            Follicle [] follicles = transform.GetComponentsInChildren<Follicle>();

            foreach(Follicle follicle in follicles)
            {
                Feelers.Add(follicle.feelerData);
            }
        }

        public void setCollidables(LayerMask mask)
        {
            collidables = mask;
        }

        public override Vector2 execute()
        {
            prevForce = combinedFeelerForce;
            combinedFeelerForce = Vector2.zero;
            Vector2 force;

            Vector2 shipPos = Helm.Position;

            // go through each feeler
            for (int i = 0; i < Feelers.Count; i++)
            {
                Feelers[i].Dir = Helm.transform.TransformDirection(MathHelperMethods.DegreeToVector2(Feelers[i].Direction));

                //Draw Debug feelers!                    
                Vector2 feelerEndPosition = shipPos + (Feelers[i].Length * Feelers[i].Dir);
                if (Helm.Controller.Cam.followTarget != null && Helm.Controller.Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                {
                    Debug.DrawLine(shipPos, feelerEndPosition, Feelers[i].Colour, Time.deltaTime, true);
                }

                RaycastHit2D hit = Physics2D.Raycast(shipPos, Feelers[i].Dir, Feelers[i].Length, collidables);
                if (hit.collider != null)
                {
                    // feeler detected collidable object
                    //overshootCollision = Vector2.Distance(feelerEndPosition, hit.point);

                    Vector2 normal = new Vector2(hit.point.x - hit.transform.position.x, hit.point.y - hit.transform.position.y).normalized;

                    force = normal * Feelers[i].Length;
                    combinedFeelerForce += force;

                    if (Helm.Controller.Cam.followTarget != null && Helm.Controller.Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                    {
                        Vector2 normalStart = new Vector2(hit.point.x, hit.point.y);
                        Vector2 normalEnd = hit.point + force;

                        Debug.DrawLine(normalStart, normalEnd, Color.white, Time.deltaTime, true);
                    }
                }
                else
                {
                    force = Vector2.zero;
                }
            }

            if (combinedFeelerForce != Vector2.zero)
            {
                if (Helm.Controller.Cam.followTarget != null && Helm.Controller.Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                {
                    Debug.DrawLine(shipPos, shipPos + combinedFeelerForce, Color.yellow, Time.deltaTime, true);
                }
            }

            prevForce = combinedFeelerForce;

            return combinedFeelerForce;
        }
    }
}