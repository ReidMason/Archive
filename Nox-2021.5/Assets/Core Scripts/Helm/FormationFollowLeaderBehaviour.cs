using UnityEngine;

namespace NoxCore.Helm
{
    public class FormationFollowLeaderBehaviour : SteeringBehaviour
    {
        [SerializeField] protected Rigidbody2D _leader;
        public Rigidbody2D Leader { get { return _leader; } set { _leader = value; } }

        [SerializeField] protected Vector2 _formationOffset;
        public Vector2 FormationOffset { get { return _formationOffset; } set { _formationOffset = value; } }

        protected ArriveBehaviour arriveBehaviour;
        protected AlignmentBehaviour alignmentBehaviour;
        protected CohesionBehaviour cohesionBehaviour;
        protected SeparationBehaviour separationBehaviour;

        public override void init()
        {
            base.init();

            arriveBehaviour = Helm.getBehaviourByName("ARRIVE") as ArriveBehaviour;
            alignmentBehaviour = Helm.getBehaviourByName("ALIGNMENT") as AlignmentBehaviour;
            cohesionBehaviour = Helm.getBehaviourByName("COHESION") as CohesionBehaviour;
            separationBehaviour = Helm.getBehaviourByName("SEPARATION") as SeparationBehaviour;
        }

        void Reset()
        {
            Label = "FOLLOW";
            SequenceID = 7;
            Weight = 50;
        }

        public override Vector2 execute()
        {
            Vector2 force = Vector2.zero;

            if (Leader != null)
            { 
                Helm.destination = Leader.transform.TransformPoint(FormationOffset);                

                if (arriveBehaviour != null)
                {
                    // calculate force to seek to point behind leader if leader is moving or arrive at formation point
                    force = arriveBehaviour.execute();

                    // multiply the force by the steering behaviour weight
                    if (force.magnitude > 0)
                    {
                        force *= arriveBehaviour.Weight;
                    }

                    if (alignmentBehaviour != null)
                    {
                        Vector2 alignmentForce = alignmentBehaviour.execute();

                        // multiply the force by the steering behaviour weight
                        if (alignmentForce.magnitude > 0)
                        {
                            alignmentForce *= alignmentBehaviour.Weight;
                            force += alignmentForce;
                        }
                    }

                    if (cohesionBehaviour != null)
                    {
                        Vector2 cohesionForce = cohesionBehaviour.execute();

                        // multiply the force by the steering behaviour weight
                        if (cohesionForce.magnitude > 0)
                        {
                            cohesionForce *= cohesionBehaviour.Weight;
                            force += cohesionForce;
                        }
                    }

                    if (separationBehaviour != null)
                    {
                        Vector2 separationForce = separationBehaviour.execute();

                        // multiply the force by the steering behaviour weight
                        if (separationForce.magnitude > 0)
                        {
                            separationForce *= separationBehaviour.Weight;
                            force += separationForce;
                        }
                    }

                    force = force.normalized * (Helm.ShipStructure.MaxSpeed * Helm.throttle);
                }               
            }

            return force;
        }
    }
}