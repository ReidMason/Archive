using UnityEngine;
using System.Collections;

using NoxCore.Debugs;

namespace NoxCore.Fittings.Modules
{
	public interface ICloakingDevice : IModule, ICloakDebuggable
	{
		bool isCloakActive();
		bool isFlippingCloak();
		void activateCloakImmediately();
		void activateCloak();
		void deactivateCloakImmediately();
		void deactivateCloak();
	}
}