using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using NoxCore.Cameras;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.GUIs;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Controllers
{
	public abstract class AIController : StructureController 
	{				
		[SerializeField]
		protected float _AITickRate;
		public float AITickRate { get { return _AITickRate; } set { _AITickRate = value; } }

        public delegate void AIControllerDelegates(AIController sender);
        public event AIControllerDelegates ControllerBoot;

        public override void boot(Structure structure, HelmController helm = null)
		{
            base.boot(structure, helm);

            // subscribe to own events
            if (structure.scanner != null)
            {
                (structure.scanner as Scanner).ScannerNewSweep += newScannerData;
            }

            // set tick rate based on structure size
            if (structure.StructureRigidbody != null)
			{
				AITickRate = structure.StructureRigidbody.mass / 5000.0f;

                // sensible AI tick rate limits
                if (AITickRate < 0.01667f) AITickRate = 0.01667f;
                else if (AITickRate > 10) AITickRate = 10;
			}
            else
            {
                switch (structure.structureSize)
                {
                    case StructureSize.TINY: AITickRate = 0.25f; break;
                    case StructureSize.SMALL: AITickRate = 0.3f; break;
                    case StructureSize.MEDIUM: AITickRate = 0.75f; break;
                    case StructureSize.LARGE: AITickRate = 1.5f; break;
                    case StructureSize.MASSIVE: AITickRate = 4; break;
                    case StructureSize.ENORMOUS: AITickRate = 6; break;
                    case StructureSize.GIGANTIC: AITickRate = 8; break;
                    case StructureSize.COLOSSAL: AITickRate = 10; break;
                    default: AITickRate = 1; break;
                }
            }
        }

        ////////////////////////////////////
        /*
			Event dispatchers for all AI controllers
		*/
        ////////////////////////////////////		

        public void Call_ControllerBoot()
        {
            if (ControllerBoot != null)
            {
                ControllerBoot(this);
            }
        }

        ///////////////////////////////////////////
        /*
			Handlers for AI controller events
		*/
        ///////////////////////////////////////////		

        protected virtual void newScannerData(Scanner sender) {}
    }
}
