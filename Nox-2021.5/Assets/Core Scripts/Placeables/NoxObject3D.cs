using UnityEngine;
using System.Collections;

namespace NoxCore.Placeables
{
	public class NoxObject3D : NoxObject, ISpawnable
	{
		protected MeshRenderer[] _objectRenderers;
		public MeshRenderer[] objectRenderers { get { return _objectRenderers; } set { _objectRenderers = value; } }
		
		protected Collider[] _objectColliders;
		public Collider[] objectColliders { get { return _objectColliders; } set { _objectColliders = value; } }
		
		protected Rigidbody[] _objectRigidbodies;
		public Rigidbody[] objectRigidbodies { get { return _objectRigidbodies; } set { _objectRigidbodies = value; } }
		
		public new virtual void spawn(bool spawnEnabled = false)
		{
			objectRenderers = GetComponentsInChildren<MeshRenderer>();
			objectColliders = GetComponentsInChildren<Collider>();
			objectRigidbodies = GetComponentsInChildren<Rigidbody>();		
			
			enableAllRenderers();
		}
		
		public virtual void despawn()
		{
			disableAllRenderers();
			disableAllColliders();
		}
		
		public virtual void respawn()
		{
			enableAllRenderers();
			enableAllColliders();		
		}
		
		public virtual void reset(){}
		
		public virtual void disableAllRenderers()
		{
			foreach(MeshRenderer renderer in objectRenderers)
			{
				renderer.enabled = false;
			}
		}		
		
		public virtual void enableAllRenderers()
		{
			foreach(MeshRenderer renderer in objectRenderers)
			{
				renderer.enabled = true;
			}
		}
		
		public virtual void disableAllColliders()
		{
			foreach(Collider collider in objectColliders)
			{
				collider.enabled = false;
			}
		}
		
		public virtual void enableAllColliders()
		{
			foreach(Collider collider in objectColliders)
			{
				collider.enabled = true;
			}
		}
		
		public virtual void hideObject()
		{
			disableAllRenderers();
			disableAllColliders();
		}
		
		public virtual void showObject()
		{
			enableAllRenderers();
			enableAllColliders();
		}
	}
}