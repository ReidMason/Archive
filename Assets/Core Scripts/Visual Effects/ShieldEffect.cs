using UnityEngine;

using NoxCore.Fittings.Modules;

namespace NoxCore.Effects
{
	public class ShieldEffect : MonoBehaviour
	{
		protected float flareTimer;
		protected Material shieldMaterial;
		protected bool shieldHit;
        protected float strength;
        protected float initialStrength;
		
		// Use this for initialization
		public void init()
		{
			shieldMaterial = GetComponent<Renderer>().material;
            strength = shieldMaterial.GetFloat("_Strength");
            initialStrength = strength;
		}

        public void reset()
        {
            enabled = true;
            shieldHit = false;
            shieldMaterial.SetFloat("_Strength", initialStrength);
        }
		
		void Update()
		{
			if (shieldHit)
			{
				flareTimer -= Time.deltaTime;		
				shieldMaterial.SetFloat("_Strength", (flareTimer * 3.5f)+1.5f);
				
				if (flareTimer <= 0) shieldHit = false;
			}
		}
		
		public void hit(float damageRatio)
		{
			if (enabled == true)
			{
				shieldHit = true;
				flareTimer = 1.0f;
				shieldMaterial.SetFloat("_Strength", (damageRatio * 3.5f)+1.5f);
			}
		}
		
		public float getShaderFloatParam(string param)
		{			
			return shieldMaterial.GetFloat(param);
		}
		
		public void setShaderFloatParam(string param, float value)
		{
			shieldMaterial.SetFloat(param, value);
		}		
	}
}