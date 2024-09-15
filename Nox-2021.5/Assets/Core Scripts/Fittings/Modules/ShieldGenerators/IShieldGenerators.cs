using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Debugs;

namespace NoxCore.Fittings.Modules
{
	public interface IShieldGenerator : IModule, IShieldDebuggable
	{
		ShieldGeneratorData ShieldGeneratorData { get; }

		float CurrentCharge { get; set; }

		float OneMinusBleedFraction { get; }

		Transform getShieldMesh();
		bool isShieldUp();
		bool isFlippingShield();
		void disable();
		void enable();
		void failed();
		void raiseShield();
		void lowerShield();
		void hit(float damageRatio);
		void resetCharge();
		void setCharge(float amount);
		void increaseCharge(float amount);
		void decreaseCharge(float amount);
	}
}