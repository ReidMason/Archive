using UnityEngine;
using System.Collections;

using NoxCore.Data.Fittings;
using NoxCore.Debugs;
using NoxCore.Effects;
using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Placeables;
using System;

namespace NoxCore.Fittings.Modules
{
	public class ShieldGenerator : Module, IShieldGenerator, IShieldDebuggable
	{
		#region variables 
		[Header("Shield Generator")]

		public ShieldGeneratorData __shieldGeneratorData;
		[NonSerialized]
		protected ShieldGeneratorData _shieldGeneratorData;
		public ShieldGeneratorData ShieldGeneratorData { get { return _shieldGeneratorData; } set { _shieldGeneratorData = value; } }

		protected float shieldUpTimer, shieldDownTimer;	

		protected float currentCharge;
		public float CurrentCharge { get { return currentCharge; } set { currentCharge = value; } }

		protected float _oneMinusBleedFraction;
		public float OneMinusBleedFraction { get { return _oneMinusBleedFraction; } }

		protected float origShieldSpeed;
		protected float origShieldStrength;

		protected bool shieldUp;
		protected bool flippingShield = false;
		
		protected Transform shieldMesh;
		protected Renderer shieldMeshRenderer;
		protected Collider shieldMeshCollider;
		protected ShieldEffect shieldEffect;
		
		protected Structure shieldStructure;

        #endregion

        #region delegates
        //Modules descend from Device
        public delegate void ShieldDelegates(ShieldGenerator sender);
        public event ShieldDelegates ShieldDropped;
        public event ShieldDelegates ShieldRaised;
        #endregion

        #region init, fitting and reset
        public override void init(DeviceData deviceData = null)
		{
			if (deviceData == null)
            {
				ShieldGeneratorData = Instantiate(__shieldGeneratorData);
				base.init(ShieldGeneratorData);
			}
            else
            {
				ShieldGeneratorData = deviceData as ShieldGeneratorData;
				base.init(deviceData);
			}

			_oneMinusBleedFraction = 1.0f - ShieldGeneratorData.BleedFraction;
			
			shieldUp = true;
			
			flippingShield = false;
			shieldUpTimer = 0;
			shieldDownTimer = 0;
			
			currentCharge = ShieldGeneratorData.MaxCharge;
			
			requiredSocketTypes.Add("SHIELDPOD");
		}
		
		public override void reset()
		{
			base.reset();
			
			shieldUp = true;
			
			flippingShield = false;
			shieldUpTimer = 0;
			shieldDownTimer = 0;
			
			currentCharge = ShieldGeneratorData.MaxCharge;
			
			shieldMeshRenderer.enabled = true;
			shieldMeshCollider.enabled = true;

            if (shieldEffect != null)
            {
                shieldEffect.reset();
            }
		}

        public override void postFitting()
        {
            base.postFitting();

            shieldStructure = getStructure();

            shieldMesh = shieldStructure.gameObject.transform.Find("ShieldMesh");

            if (shieldMesh != null)
            {
                shieldMeshRenderer = shieldMesh.GetComponent<Renderer>();
                shieldMeshCollider = shieldMesh.GetComponent<Collider>();

                if (DeviceData.ActiveOnSpawn == true)
                {
                    shieldUp = true;
                    shieldMeshRenderer.enabled = true;
                    shieldMeshCollider.enabled = true;
                }
                else
                {
                    shieldUp = false;
                    shieldMeshRenderer.enabled = false;
                    shieldMeshCollider.enabled = false;
                }

                shieldEffect = shieldMesh.GetComponent<ShieldEffect>();

                if (shieldEffect != null)
                {
                    shieldEffect.init();

                    origShieldSpeed = shieldEffect.getShaderFloatParam("_Speed");
                    origShieldSpeed = Mathf.Clamp(origShieldSpeed, 0.5f, origShieldSpeed);

                    origShieldStrength = shieldEffect.getShaderFloatParam("_Strength");
                }
            }
        }
        #endregion

        #region getters	

		public Transform getShieldMesh()
		{
			return shieldMesh;
		}
		
		public bool isShieldUp()
		{
			return shieldUp;
		}
		
		public bool isFlippingShield()
		{
			return flippingShield;
		}
        #endregion

        #region disable and enable
        public void disable()
		{
			shieldUp = false;
			shieldMeshRenderer.enabled = false;
			shieldMeshCollider.enabled = false;
			shieldEffect.enabled = false;
            Call_ShieldDropped();
        }
		
		public void enable()
		{
			shieldUp = true;
			shieldMeshRenderer.enabled = true;
			shieldMeshCollider.enabled = true;
			shieldEffect.enabled = true;
            Call_ShieldRaised();
        }
        #endregion

        #region Shield Status
        public void failed()
		{
			CurrentCharge = 0;
			flippingShield = false;
			shieldUp = false;
			shieldUpTimer = 0;
			shieldDownTimer = 0;

            disable();

            NoxGUI.Instance.setMessage(structure.Name + " has lost a shield!");
        }
		
		public void raiseShield()
		{
			if (isActiveOn() == true && isFlippingActivation() == false && isFlippingShield() == false && shieldUp == false && destroyed == false && currentCharge >= ShieldGeneratorData.MinCharge)
			{
				StartCoroutine (beginRaisingShield());
			}
		}	
		
		protected IEnumerator beginRaisingShield()
		{
			// a shield can only be raised if it is at least at its minimum charge and is not currently up
			
			flippingShield = true;
			
			while (shieldUp == false)
			{
				if (!(isActiveOn() == true && isFlippingActivation() == false)) // what if the generator is switched off mid-way through raising?
				{
					shieldUpTimer = 0;
					shieldUp = false;
					flippingShield = false;
					break;
				}                
				
				if (shieldUpTimer < ShieldGeneratorData.ShieldDelay)
				{
					shieldUpTimer += Time.deltaTime;
					yield return null;
				}
				else if (currentCharge >= ShieldGeneratorData.MinCharge)
				{
					shieldUpTimer = 0;
                    enable();
					
					flippingShield = false;
				}
			}
		}
		
		public void lowerShield()
		{
			if (isActiveOn() == true && isFlippingActivation() == false && isFlippingShield() == false && shieldUp == true && destroyed == false)
			{            
				StartCoroutine (beginLoweringShield());
			}
		}	
		
		protected IEnumerator beginLoweringShield()
		{
			// a shield can only be lowered if it is currently up
			
			flippingShield = true;
			
			while(shieldUp == true)
			{
				if (shieldDownTimer < ShieldGeneratorData.ShieldDelay)
				{					
					shieldDownTimer += Time.deltaTime;
					yield return null;
				}
				else
				{
					shieldDownTimer = 0;
					
					disable();
					
					flippingShield = false;                    
				}
			}
		}
#endregion

        #region Charge
        public void resetCharge()
		{
			CurrentCharge = ShieldGeneratorData.MaxCharge;
        }		
		
		public void setCharge(float amount)
		{
			CurrentCharge = amount;
			CurrentCharge = Mathf.Clamp(CurrentCharge, 0, ShieldGeneratorData.MaxCharge);
        }
		
		public void increaseCharge(float amount)
		{
			if (CurrentCharge < ShieldGeneratorData.MaxCharge && isDestroyed() == false)
			{
				CurrentCharge = Mathf.Min(CurrentCharge + amount, ShieldGeneratorData.MaxCharge);
            }	
		}
		
		public void decreaseCharge(float amount)
		{
			if (CurrentCharge > 0 && isDestroyed() == false)
			{
				CurrentCharge = Mathf.Max(CurrentCharge - amount, 0);
				
				if (CurrentCharge == 0)
				{					
					failed();
				}
			}
		}
        #endregion

        #region Damage/Destroy
        public void hit(float damageRatio)
        {
            shieldEffect.hit(damageRatio);
        }

        public override void destroy()
		{
			disable();
			
			base.destroy();
		}
		
		public override void explode(int repeatedNumExplosions = 0)
		{
			failed();
			
			base.explode(repeatedNumExplosions);
		}
        #endregion

        #region update
        public override void update()
		{
			base.update();
			
			if (isActiveOn() == true && isFlippingActivation() == false)
			{
				if (shieldUp == true)
				{
					if (currentCharge > 0)
					{
						// render and update shield effect
						enable();
					}
					else
					{
						// do not render or update shield effect
						disable();
					}
				}
				
				// if shield is active, regenerate charge
				if (CurrentCharge < ShieldGeneratorData.MaxCharge)
				{
					CurrentCharge += Mathf.Min(ShieldGeneratorData.RechargeRate * Time.deltaTime, ShieldGeneratorData.MaxCharge - CurrentCharge);
					
					shieldEffect.setShaderFloatParam("_Speed", ((CurrentCharge / ShieldGeneratorData.MaxCharge) * origShieldSpeed));
					shieldEffect.setShaderFloatParam("_Strength", ((CurrentCharge / ShieldGeneratorData.MaxCharge) * origShieldStrength));
				}
			}
			else
			{
				if (CurrentCharge > 0)
				{
					CurrentCharge = Mathf.Max(0, CurrentCharge - (ShieldGeneratorData.RechargeRate * Time.deltaTime));
					
					shieldEffect.setShaderFloatParam("_Speed", ((CurrentCharge / ShieldGeneratorData.MaxCharge) * origShieldSpeed));
					shieldEffect.setShaderFloatParam("_Strength", ((CurrentCharge / ShieldGeneratorData.MaxCharge) * origShieldStrength));
				}
				else
				{
					// do not render or update shield effect
					if (shieldUp == true) failed();
				}
			}

            shieldStructure.ShieldStrength += CurrentCharge;
            shieldStructure.MaxShieldStrength += ShieldGeneratorData.MaxCharge;
        }
		#endregion

		#region debug

		public void debugIncrease(object sender, DebugEventArgs args, float amount)
		{
			increaseCharge(amount);
			NoxGUI.Instance.setMessage("DEBUG: " + ShieldGeneratorData.Type + ":" + ShieldGeneratorData.SubType + " has increased its charge by " + amount);
		}

		public void debugDecrease(object sender, DebugEventArgs args, float amount)
		{
			decreaseCharge(amount);
			NoxGUI.Instance.setMessage("DEBUG: " + ShieldGeneratorData.Type + ":" + ShieldGeneratorData.SubType + " has decreased its charge by " + amount);
		}

		public void debugRaise(object sender, DebugEventArgs args)
		{
			raiseShield();
            NoxGUI.Instance.setMessage("DEBUG: " + ShieldGeneratorData.Type + ":" + ShieldGeneratorData.SubType + " has been raised");
		}		
		
		public void debugLower(object sender, DebugEventArgs args)
		{
			lowerShield();
            NoxGUI.Instance.setMessage("DEBUG: " + ShieldGeneratorData.Type + ":" + ShieldGeneratorData.SubType + " has been lowered");
		}		
		
		public void debugFail(object sender, DebugEventArgs args)
		{
			failed();
            NoxGUI.Instance.setMessage("DEBUG: " + ShieldGeneratorData.Type + ":" + ShieldGeneratorData.SubType + " has failed");
		}
        #endregion

        #region Event Delegates
        public void Call_ShieldDropped()
        {
            if (ShieldDropped != null)
            {
                Call_ShieldDropped();
            }            
        }

        public void Call_ShieldRaised()
        {
            if (ShieldRaised != null)
            {
                ShieldRaised(this);
            }
        }
        #endregion
    }
}