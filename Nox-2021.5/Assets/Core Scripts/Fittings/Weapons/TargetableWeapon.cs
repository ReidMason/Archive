using UnityEngine;

using NoxCore.Fittings.Modules;
using NoxCore.Placeables;
using NoxCore.Utilities;
using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Weapons
{
    public abstract class TargetableWeapon : Weapon, ITargetable
    {
        #region variables        
        [SerializeField]
        protected (GameObject structure, GameObject system)? prevTarget;

        [SerializeField]
        protected (GameObject structure, GameObject system)? prevLockedTarget;

        [SerializeField]
        protected (GameObject structure, GameObject system)? _Target;
        public (GameObject structure, GameObject system)? Target { get { return _Target; } set { _Target = value; } }

        [SerializeField]
        protected (GameObject structure, GameObject system)? _LockedTarget;
        public (GameObject structure, GameObject system)? LockedTarget { get { return _LockedTarget; } set { _LockedTarget = value; } }

        [SerializeField]
        protected (Structure structure, Module module)? _LockedTargetInfo;
        public (Structure structure, Module module)? LockedTargetInfo { get { return _LockedTargetInfo; } set { _LockedTargetInfo = value; } }

        [Header("Targeting")]
        [SerializeField]
        [ShowOnly]
        protected bool _TargetIsAcquired;
        public bool TargetIsAcquired { get { return _TargetIsAcquired; } set { _TargetIsAcquired = value; } }

        [SerializeField]
        [ShowOnly]
        protected bool _TargetIsLocked;
        public bool TargetIsLocked { get { return _TargetIsLocked; } set { _TargetIsLocked = value; } }

        [SerializeField]
        [ShowOnly]
        protected float? distanceToTarget;
        #endregion

        #region delegates
        //Inherits from Device & Module & Weapon
        public event WeaponEventDispatcher TargetLocked;
        #endregion

        public override void init(DeviceData deviceData = null)
        {
            base.init(deviceData);

            prevTarget = null;
            prevLockedTarget = null;

            Target = null;
            LockedTarget = null;
            LockedTargetInfo = null;

            requiredSocketTypes.Add("TARGETABLE");
        }

        public override void reset()
        {
            base.reset();

            prevTarget = null;
            prevLockedTarget = null;

            Target = null;
            LockedTarget = null;
            LockedTargetInfo = null;

            TargetIsLocked = false;
            TargetIsAcquired = false;
        }

        #region destroy
        public override void destroy()
        {
            prevTarget = null;
            prevLockedTarget = null;

            Target = null;
            LockedTarget = null;
            LockedTargetInfo = null;

            TargetIsLocked = false;
            TargetIsAcquired = false;

            base.destroy();
        }

        public override void explode(int repeatedNumExplosions = 0)
        {
            prevTarget = null;
            prevLockedTarget = null;

            Target = null;
            LockedTarget = null;
            LockedTargetInfo = null;

            TargetIsLocked = false;
            TargetIsAcquired = false;

            base.explode(repeatedNumExplosions);
        }
        #endregion

        public override float getDamage()
        {
            if (distanceToTarget == null || distanceToTarget.Value < WeaponData.MinRange || distanceToTarget.Value > WeaponData.MaxRange)
            {
                return 0;
            }

            return WeaponData.BaseDamage;
        }

        #region target
        public (GameObject structure, GameObject system)? getTarget()
        {
            return Target;
        }

        public void setTarget((GameObject structure, GameObject system) target)
        {
            this.Target = target;
        }

        protected float? getDistanceToTarget()
        {
            return distanceToTarget;
        }
        protected void calculateDistanceToTarget()
        {
            if (LockedTarget != null)
            {
                (GameObject structure, GameObject system) lockedTarget = LockedTarget.GetValueOrDefault();

                if (lockedTarget.structure != null)
                {
                    if (lockedTarget.system == null)
                    {
                        distanceToTarget = Vector2.Distance(gameObject.transform.position, lockedTarget.structure.transform.position);
                    }
                    else
                    {
                        distanceToTarget = Vector2.Distance(gameObject.transform.position, lockedTarget.system.transform.position);
                    }
                }
                else
                {
                    distanceToTarget = null;
                }
            }
            else
            {
                distanceToTarget = null;
            }
        }

        public virtual bool acquireTarget(GameObject targetStructure, GameObject targetModule)
        {
            (GameObject structure, GameObject system) acquisitionTarget = (targetStructure, targetModule);

            if (Target == null)
            {
                Target = acquisitionTarget;
                TargetIsAcquired = true;
                TargetIsLocked = false;

                /*
				if (this.target._2 == null)
				{
					// Debug.Log ("Acquired target: " + targetStructure.name);
				}
				else
				{
					// Debug.Log ("Acquired target: " + targetStructure.name + " - " + targetModule.name);
				}				
				*/

                return true;
            }
            else
            {
                (GameObject structure, GameObject system) target = Target.GetValueOrDefault();

                if (target.structure != acquisitionTarget.structure || target.system != acquisitionTarget.system)
                {
                    Target = acquisitionTarget;
                    TargetIsAcquired = true;
                    TargetIsLocked = false;

                    /*
				    if (this.target._2 == null)
				    {
					    // Debug.Log ("Acquired target: " + targetStructure.name);
				    }
				    else
				    {
					    // Debug.Log ("Acquired target: " + targetStructure.name + " - " + targetModule.name);
				    }				
				    */

                    return true;
                }
            }

            return false;
        }

        public virtual void unacquireTarget()
        {
            unlockTarget();

            Target = null;
            TargetIsAcquired = false;
        }

        protected virtual void lockTarget()
        {
            if (Target != null)
            {
                (GameObject structure, GameObject system) target = Target.GetValueOrDefault();

                if (target.structure != null)
                {
                    if (Target != prevTarget)
                    {
                        // Debug.Log ("Weapon has locked");

                        /*
					    if (Target._2 == null)
					    {
						    D.log("Weapon", Type + " on " + structure.gameObject.name + " has locked to " + Target._1.name);
					    }
					    else
					    {
						    D.log("Weapon", Type + " on " + structure.gameObject.name + " has locked to " + Target._2.name + " on " + Target._1.name);
					    }
                        */

                        prevLockedTarget = LockedTarget;
                        LockedTarget = Target;

                        (GameObject structure, GameObject system) lockedTarget = target;

                        if (lockedTarget.system == null)
                        {
                            LockedTargetInfo = (lockedTarget.structure.GetComponent<Structure>(), null);
                        }
                        else
                        {
                            LockedTargetInfo = (lockedTarget.structure.GetComponent<Structure>(), lockedTarget.system.GetComponent<Module>());
                        }
                    }

                    TargetIsLocked = true;
                    Call_TargetLocked();
                }
            }
        }

        protected virtual void unlockTarget()
        {
            if (LockedTarget != null)
            {
                // Debug.Log ("Weapon has unlocked");
                // D.log("Weapon", DeviceName + " on " + structure.gameObject.name + " has unlocked");

                prevLockedTarget = null;
                LockedTarget = null;
                LockedTargetInfo = null;

                TargetIsLocked = false;

                //firing = false;
            }
        }
        #endregion

        protected override bool canFire()
        {
            if (base.canFire() == true)
            {
                calculateDistanceToTarget();

                // is the target valid and is in range of the weapon?
                if (distanceToTarget != null && distanceToTarget.Value >= WeaponData.MinRange && distanceToTarget.Value <= WeaponData.MaxRange)
                {
                    return true;
                }
            }

            return false;
        }

        #region Event Handlers
        public void Call_TargetLocked()
        {
            if (TargetLocked != null)
            {
                TargetLocked(this, new WeaponFiredEventArgs(this));
            }
        }
        #endregion
    }
}