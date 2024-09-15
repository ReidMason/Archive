using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    public abstract class ProjectileMovement : MonoBehaviour
    {
        protected Rigidbody2D myRigidbody;
        public Rigidbody2D MyRigidbody { get { return myRigidbody; } }

        public float rotationSpeed;
        protected Quaternion lookRotation;
        protected Vector3 direction;

        protected float flightSpeed;

        public virtual void init()
        {
            myRigidbody = GetComponent<Rigidbody2D>();
        }

        void OnEnable()
        {
            myRigidbody.WakeUp();
        }

        void OnDisable()
        {
            myRigidbody.Sleep();
        }

        public void setInitVelocity(Vector3 initVelocity)
        {
            flightSpeed = initVelocity.magnitude;
            MyRigidbody.velocity = initVelocity;
        }

        public void disable()
        {
            myRigidbody.Sleep();
        }
    }
}