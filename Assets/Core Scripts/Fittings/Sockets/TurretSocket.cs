using UnityEngine;
using System.Collections;
using System;
using System.Text;

using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;

namespace NoxCore.Fittings.Sockets
{
	public class TurretSocket : WeaponSocket
	{
        public Color spreadColour;
		public bool fixedFiringArc;
		
		[Range (0, 180)]
		public float fireArcHalf;		
		private float _fireArc;
		public float FireArc { get { return _fireArc; } }	
		
        public override void postFitting()
        {
            if (fireArcHalf != 180)
            {
                fixedFiringArc = true;
            }

            _fireArc = fireArcHalf * 2.0f;
        }

        public override StructureSocketInfo getSocketInfo()
        {
            StructureSocketInfo socketInfo = base.getSocketInfo();

            TurretSocketInfo turretSocketInfo = TurretSocketInfo.CopyToTurretSocketInfo(socketInfo);
            
            turretSocketInfo.fixedFiringArc = fixedFiringArc;
            turretSocketInfo.fireArcHalf = fireArcHalf;

            return turretSocketInfo;
        }

        #if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Weapon weapon = InstalledModule as Weapon;

            if (weapon != null)
            {
                UnityEditor.Handles.color = spreadColour;
                Vector3 leftMost = Quaternion.AngleAxis(fireArcHalf, Vector3.forward) * transform.up;
                UnityEditor.Handles.DrawSolidArc(transform.position, -Vector3.forward, leftMost, _fireArc, weapon.WeaponData.MaxRange);
            }
        }
        #endif
    }
}