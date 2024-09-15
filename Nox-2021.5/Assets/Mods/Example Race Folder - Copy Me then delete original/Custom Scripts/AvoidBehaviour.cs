using UnityEngine;
using System.Collections.Generic;

using NoxCore.Data;
using NoxCore.Utilities;
using NoxCore.Helm;

namespace Example
{
    public class AvoidBehaviour : SteeringBehaviour
    {
        [SerializeField] protected List<Feeler> _feelers;
        public List<Feeler> Feelers { get { return _feelers; } set { _feelers = value; } }

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
            length = transform.parent.GetComponent<PolygonCollider2D>().bounds.extents.y;
            width = transform.parent.GetComponent<PolygonCollider2D>().bounds.extents.x;
        }

        public override void Start()
        {
            base.Start();

            combinedFeelerForce = Vector2.zero;
            shipRadius = Mathf.Max(length, width);

            // orient the feelers so that a direction of 0 is straight ahead
            foreach(Feeler feeler in Feelers)
            {
                feeler.Direction = feeler.direction + 90;
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
                Vector2 feelerEndPosition = shipPos + (Feelers[i].length * Feelers[i].Dir);
                if (Helm.Controller.Cam.followTarget != null && Helm.Controller.Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                {
                    Debug.DrawLine(shipPos, feelerEndPosition, Feelers[i].colour, Time.deltaTime, true);
                }
                    
                RaycastHit2D hit = Physics2D.Raycast(shipPos, Feelers[i].Dir, Feelers[i].length, collidables);
                if (hit.collider != null)
                {
                    // feeler detected collidable object
                    //overshootCollision = Vector2.Distance(feelerEndPosition, hit.point);

                    Vector2 normal = new Vector2(hit.point.x - hit.transform.position.x, hit.point.y - hit.transform.position.y).normalized;

                    force = normal * Feelers[i].length;
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