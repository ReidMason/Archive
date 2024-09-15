using UnityEngine;

using System;

using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Placeables;
using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Weapons
{
	public abstract class RotatingTurret : TargetableWeapon, IRotatingTurret, IDamageModifier
	{
        [Header("Rotating Turret")]

        [NonSerialized]
        protected RotatingTurretData _rotatingTurretData;
        public RotatingTurretData RotatingTurretData { get { return _rotatingTurretData; } set { _rotatingTurretData = value; } }

        // used for deflection shooting
        protected Vector2? targetPosition;
        public Vector2? TargetPosition { get { return targetPosition; } set { targetPosition = value; } }

        protected Vector2 targetDirection;

        protected Transform muzzle;

        public override void init(DeviceData deviceData = null)
		{
            RotatingTurretData = deviceData as RotatingTurretData;

            base.init(deviceData);

            requiredSocketTypes.Add("TURRET");

            muzzle = transform.Find("Muzzle");
            if (muzzle == null) muzzle = transform;
        }

        public override Transform getFirePoint()
        {
            return muzzle;
        }

        protected (float distance, float radialVel, float transversalVel) calculateDistanceRadialTransversal()
		{
			float distance, radial, transversal;
			Vector2 relativeVelocity;
			
			Structure targetStructure = LockedTarget.GetValueOrDefault().structure.GetComponent<Structure>();
			
			Rigidbody2D targetStructureRigidbody = targetStructure.StructureRigidbody;
			Rigidbody2D weaponStructureRigidbody = structure.StructureRigidbody;
			
			if (targetStructureRigidbody == null || weaponStructureRigidbody == null)
			{
				if (targetStructureRigidbody == null && weaponStructureRigidbody == null)
				{
					relativeVelocity = Vector2.zero;
				}
				else if (targetStructureRigidbody == null)
				{
					relativeVelocity = -weaponStructureRigidbody.velocity;
				}
				else
				{
					relativeVelocity = targetStructureRigidbody.velocity;
				}
			}
			else
			{
				relativeVelocity = targetStructureRigidbody.velocity - weaponStructureRigidbody.velocity;
			}
			
			Vector2 relativePosition = targetStructure.transform.position - structure.transform.position;
			
			float relativeSpeed = relativeVelocity.magnitude;
			
			distance = relativePosition.magnitude;
			
			// if no relative velocity then no transversal velocity
			if (relativeSpeed == 0 || distance == 0)
			{
				radial = 0;
				transversal = 0;
			}
			else
			{
				float theta = Mathf.Acos(Vector2.Dot(relativeVelocity.normalized, relativePosition.normalized));
				radial = relativeSpeed * Mathf.Cos(theta);
				transversal = relativeSpeed * Mathf.Sin(theta) / distance;
			}
			
			return (distance, radial, transversal);
		}				
		
		// calculate maximum transversal velocity based on weapon's slew speed during this update, distance to target and structure size to weapon size ratio
		public virtual float damageModifier(GameObject collidedObject, float damage, Weapon weapon, (GameObject structure, GameObject system)? target, Projectile projectile = null)
		{
			float damageModifier = 1.0f;

			// calculate distance and velocity vector
			if (RotatingTurretData.TransversalDamage == true)
			{
				(float distance, float radialVel, float transversalVel) weaponHitInfo = calculateDistanceRadialTransversal();
			
				float maxTransversal = Mathf.Tan(RotatingTurretData.TrackingAngle) * (ModuleData.AspectRadius / 10.0f) * weaponHitInfo.distance * Time.deltaTime;
			
				// Debug.Log ("Transversal: " + maxTransversal);
				
				// if transversal velocity is non-zero then calculate damageModifier and clamp between 1 & 0
				if (weaponHitInfo.transversalVel != 0)
				{
					damageModifier = Mathf.Max(Mathf.Min(maxTransversal / weaponHitInfo.transversalVel, 1), 0);
					
					// D.log("Structure", "Damage Modifer: " + damageModifier);
				}		
				
				damage *= damageModifier;
			}
			
			return damage;
		}
		
		public bool isWithinFireArc(GameObject go)
		{
			TurretSocket turretSocket = Socket as TurretSocket;
			
			if (turretSocket != null)
			{
				if (turretSocket.fixedFiringArc == true)
				{										
					if (Vector2.Angle(go.transform.position - transform.position, turretSocket.transform.up) <= turretSocket.fireArcHalf)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			
			return true;
		}
		
        protected void rotateToVector(Vector2 v)
        {
            Debug.DrawLine(transform.position, (Vector2)(transform.position) + (v.normalized * 25), Color.blue);

            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg - 90;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), RotatingTurretData.SlewSpeed * Time.deltaTime);
        }

		public void rotate()
		{
			TurretSocket turretSocket = Socket as TurretSocket;

            #region no turret socket
            if (turretSocket == null)
            {
                // this should be impossible unless the fitting is wrong
                rotateToVector(structure.transform.up);
                unacquireTarget();
                return;
            }
            #endregion

            #region rotate to local forward

            (GameObject structure, GameObject system) target = Target.GetValueOrDefault();

            if (Target == null || target.structure == null)
            {
                // rotate back to local forward (turret or structure)
                if (turretSocket.fixedFiringArc == true)
                {
                    rotateToVector(turretSocket.transform.up);
                }
                else
                {
                    rotateToVector(structure.transform.up);
                }

                unacquireTarget();
                return;
            }
            #endregion

            #region check target range
            // if the target exists get its apparent distance else rotate back to socket default (forward)
            float targetDistance;

            if (targetPosition == null)
            {
                if (target.system == null)
                {
                    targetDirection = target.structure.transform.position - transform.position;
                    targetDistance = Vector2.Distance(target.structure.transform.position, transform.position);
                }
                else
                {
                    targetDirection = target.system.transform.position - transform.position;
                    targetDistance = Vector2.Distance(target.system.transform.position, transform.position);
                }
            }
            else
            {
                Vector2 targetPos = targetPosition.GetValueOrDefault();
                targetDirection = targetPos - (Vector2)transform.position;
                targetDistance = Vector2.Distance(targetPos, transform.position);
            }

            // check if the target is out of range and rotate to socket default (local up vector) if so
            if (targetDistance > WeaponData.MaxRange && Target != null)
            {
                if (turretSocket != null)
                {
                    if (turretSocket.fixedFiringArc == true)
                    {
                        rotateToVector(turretSocket.transform.up);
                    }
                    else
                    {
                        rotateToVector(structure.transform.up);
                    }
                }
                else
                {
                    rotateToVector(structure.transform.up);
                }

                // Debug.Log ("Weapon unlocked due to range");
                unlockTarget();
                return;
            }
            #endregion

            #region rotate turret
            if (turretSocket != null && turretSocket.fixedFiringArc == true)
            {
                float angleBetween = Vector2.Angle(targetDirection, turretSocket.transform.up);

                if (angleBetween <= turretSocket.fireArcHalf)
                {
                    rotateToVector(targetDirection);
                }
                else
                {
                    Vector3 cross = Vector3.Cross(turretSocket.transform.up, targetDirection);

                    if (cross.z > 0)
                    {
                        angleBetween = 360 - angleBetween;
                    }

                    if (angleBetween < 0)
                    {
                        rotateToVector(Quaternion.AngleAxis(-turretSocket.fireArcHalf, Vector3.forward) * turretSocket.transform.up);
                    }
                    else
                    {
                        rotateToVector(Quaternion.AngleAxis(turretSocket.fireArcHalf, Vector3.forward) * turretSocket.transform.up);
                    }
                }
            }
            else
            {
                // rotate non-turret or 360deg turret
                rotateToVector(targetDirection);
            }
            #endregion
        }

        protected void setTurretLock()
        {
            if (TargetIsAcquired == true && Target != null)
            {
                // default value for the acquisition angle hedge
                float acquisitionAngle = 1.0f;

                (GameObject structure, GameObject system) target = Target.GetValueOrDefault();

                if (target.system == null)
                {
                    Structure targetStructure = target.structure.GetComponent<Structure>();

                    if (targetStructure != null)
                    {
                        if (targetStructure.StructureData != null)
                        {
                            acquisitionAngle = targetStructure.StructureData.AspectRadius * RotatingTurretData.TrackingAngle;
                        }
                        else
                        {
                            acquisitionAngle = (targetStructure.getStructureSizeLimit(targetStructure.structureSize) / 10.0f) * RotatingTurretData.TrackingAngle;
                        }
                    }
                }
                else
                {
                    acquisitionAngle = target.system.GetComponent<Module>().ModuleData.AspectRadius * RotatingTurretData.TrackingAngle;
                }

                float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90;

                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

                float angleDiff = Quaternion.Angle(transform.rotation, targetRotation);

                if (angleDiff <= acquisitionAngle)
                {
                    // TODO - what about a lock timer?
                    if (LockedTarget != Target)
                    {
                        lockTarget();
                    }
                }
                else if (LockedTarget != null)
                {
                    if (TargetIsLocked == true)
                    {
                        // Debug.Log ("Weapon unlocked due to turret angle");
                        unlockTarget();
                    }
                }
            }

            if (TargetIsLocked == true)
            {
                Debug.DrawLine(transform.position, transform.position + (transform.up.normalized * 35), Color.red);
            }
            else
            {
                Debug.DrawLine(transform.position, transform.position + (transform.up.normalized * 30), Color.green);
            }
        }

        public override void update()
		{
			base.update();
			
			if (isActiveOn() == true)
			{
				rotate();
                setTurretLock();
			}
		}
	}
}