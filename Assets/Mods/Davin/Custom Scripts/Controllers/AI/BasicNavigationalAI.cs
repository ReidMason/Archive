using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Rules;
using NoxCore.Utilities;

namespace NoxCore.Controllers
{
	public class BasicNavigationalAI : AIStateController 
	{
		SeekBehaviour seekBehaviour;

		public override void boot(Structure structure, HelmController helm = null)
		{
			base.boot(structure, helm);

			aiActions.Add("NAVIGATE", navigateAction);

			state = "NAVIGATE";

			seekBehaviour = helm.getBehaviourByName("SEEK") as SeekBehaviour;

			booted = true;
		}	

		protected virtual Vector2? setHelmDestination()
		{
			if (ArenaRules.radius != Mathf.Infinity)
			{
				return Random.insideUnitCircle * ArenaRules.radius * 0.667f;
			}
			else
			{
				return Random.insideUnitCircle * 2000 * 0.667f;
			}
		}	

		public virtual string navigateAction()
		{			
			if (seekBehaviour != null)
			{
				if (seekBehaviour.Active == false)
				{
					seekBehaviour.enableExclusively();
				}

				if (Helm.destination == null)
				{
					Helm.destination = setHelmDestination().GetValueOrDefault();
				}

				if (ArenaRules.radius > 0)
				{
					if (Helm.destination.GetValueOrDefault().magnitude > ArenaRules.radius)
					{
						Helm.destination = null;
					}
				}

				if (Helm.destination != null && Cam.followTarget != null && Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
				{
					Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
				}
			}
			
			return "NAVIGATE";
		}
	}
}
