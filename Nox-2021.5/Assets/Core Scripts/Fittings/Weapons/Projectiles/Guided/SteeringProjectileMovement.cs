using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoxCore.Fittings.Weapons
{
    public class SteeringProjectileMovement : ProjectileMovement
    {
        protected GameObject lockedTarget;
        public GameObject LockedTarget { get { return lockedTarget; } set { lockedTarget = value; } }

        protected Vector2 targetLastPosition;
        public Vector2 TargetLastPosition { get { return targetLastPosition; } set { targetLastPosition = value; } }

        public Vector2 correctVelocity(Vector2 currentVelocity, Vector2 requestedVelocity, float maxTurn)
        {
            float Bearing = (Mathf.Atan2(-currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg) + 90;

            if (Bearing < 0) Bearing += 360;

            float newBearing;

            if (isTurningLeft(currentVelocity, requestedVelocity) == true)
            {
                newBearing = (Mathf.Deg2Rad * Bearing) - maxTurn;
                // Debug.Log ("Turning left - current bearing: " + Bearing + "   new bearing: " + newBearing);
            }
            else
            {
                newBearing = (Mathf.Deg2Rad * Bearing) + maxTurn;
                // Debug.Log ("Turning right - current bearing: " + Bearing + "   new bearing: " + newBearing);
            }

            return new Vector2(requestedVelocity.magnitude * Mathf.Sin(newBearing), requestedVelocity.magnitude * Mathf.Cos(newBearing));
        }

        public bool isTurningLeft(Vector2 currentVelocity, Vector2 newVelocity)
        {
            if (currentVelocity.x * newVelocity.y - currentVelocity.y * newVelocity.x > 0) return true;
            else return false;
        }

        void Update()
        {
            float Bearing = (Mathf.Atan2(-myRigidbody.velocity.y, myRigidbody.velocity.x) * Mathf.Rad2Deg) + 90;

            if (Bearing < 0) Bearing += 360;

            transform.rotation = Quaternion.Euler(0, 0, -Bearing);
        }

        void FixedUpdate()
        {
            Vector2 steeringVector;

            if (lockedTarget != null)
            {
                steeringVector = lockedTarget.transform.position - transform.position;
            }
            else
            {
                steeringVector = TargetLastPosition - (Vector2)(transform.position);
            }

            Vector2 newVelocity = steeringVector.normalized * flightSpeed;
            Vector2 steeringForce = newVelocity - myRigidbody.velocity;

            Vector2 acceleration = steeringForce / myRigidbody.mass;

            // Debug.Log ("Acceleration: " + acceleration + "   mag: " + acceleration.magnitude);

            // get desired change in velocity
            newVelocity = myRigidbody.velocity + (acceleration * Time.deltaTime);

            // Debug.Log ("Pre Velocity: " + newVelocity.magnitude);

            // correct force based on maximum turning rate
            if (newVelocity.magnitude > 0)
            {
                float maxTurn = rotationSpeed * Time.deltaTime;

                float angleToDestination = Vector2.Angle(newVelocity, myRigidbody.velocity);

                if (angleToDestination * Mathf.Deg2Rad > maxTurn)
                {
                    // Debug.Log ("Turn too great. Angle: " + angleToDestination * Mathf.Rad2Deg + "   Max possible: " + maxTurn * Mathf.Rad2Deg);
                    // Debug.Log ("Current velocity: " + transform.rigidbody.velocity + "   requested velocity: " + newVelocity);				
                    newVelocity = correctVelocity(myRigidbody.velocity, newVelocity, maxTurn);
                    // Debug.Log ("Corrected velocity: " + newVelocity);	
                }
            }

            // limit ship speed to maximum velocity
            if (newVelocity.magnitude > flightSpeed)
            {
                // Debug.Log ("Velocity too high: " + newVelocity + "   mag: " + newVelocity.magnitude);
                newVelocity = newVelocity.normalized * flightSpeed;
                // Debug.Log ("Velocity reduced to: " + newVelocity + "   mag: " + newVelocity.magnitude);
            }

            // set new velocity
            myRigidbody.velocity = newVelocity;
        }
    }
}