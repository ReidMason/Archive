using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Collections.Generic;
using System.Collections;

// some methods taken from http://www.gamasutra.com/blogs/JoshSutphin/20131007/201829/Adding_to_Unitys_BuiltIn_Classes_Using_Extension_Methods.php
namespace NoxCore.Utilities
{
	public static class UnityExtensionMethods
	{
		//Even though they are used like normal methods, extension
		//methods must be declared static. Notice that the first
		//parameter has the 'this' keyword followed by a class
		//variable. This variable denotes which class the extension
		//method becomes a part of.
		public static void ResetTransformation(this Transform trans)
		{
			trans.position = Vector3.zero;
			trans.localRotation = Quaternion.identity;
			trans.localScale = new Vector3(1, 1, 1);
		}
		
		public static void SetCollisionRecursively(this GameObject gameObject, bool tf)
		{
			Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
			foreach(Collider collider in colliders)
				collider.enabled = tf;
		}	
		
		public static void SetVisualRecursively(this GameObject gameObject, bool tf)
		{
			Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
			foreach(Renderer renderer in renderers)
				renderer.enabled = tf;
		}
		
		public static void SetComponentRecursively<T>(this GameObject gameObject, bool tf) where T : Component
		{
			T[] comps = gameObject.GetComponentsInChildren<T>();
			foreach (T comp in comps)
			{
				
				try
				{
					System.Reflection.PropertyInfo pi = (typeof(T)).GetProperty("enabled");
					if (null != pi && pi.CanWrite)
					{
						pi.SetValue(comp, tf, null);
					}
					else
					{
						Console.WriteLine("Property does not exist, or cannot write");
					}
				}
				catch (NullReferenceException e)
				{
					Console.WriteLine("The property does not exist in Component." + e.Message);
				}
			}
		}	

        public static Transform FindChildStartsWith(this Transform t, string substring)
        {
            foreach(Transform child in t)
            {
                if (child.name.StartsWith(substring))
                {
                    return child;
                }
            }

            return null;
        }

        public static Transform FindChildWithTag(this Transform t, string tag)
        {
            foreach (Transform child in t)
            {
                if (child.tag == tag)
                {
                    return child;
                }
            }

            return null;
        }

        public static T GetComponentInChildrenWithTag<T>(this GameObject gameObject, string tag) where T : Component
		{
			foreach(Transform t in gameObject.transform)
			{
				if(t.CompareTag(tag))
					return t.GetComponent<T>();
			}
			
			return null;
		}	
				
		public static T[] GetComponentsInChildrenWithTag<T>(this GameObject gameObject, string tag) where T : Component
		{
			List<T> results = new List<T>();
			
			if(gameObject.CompareTag(tag))
				results.Add(gameObject.GetComponent<T>());
			
			foreach(Transform t in gameObject.transform)
				results.AddRange(t.gameObject.GetComponentsInChildrenWithTag<T>(tag));
			
			return results.ToArray();
		}
		
		public static T GetComponentInParents<T>(this GameObject gameObject) where T : Component
		{
			for(Transform t = gameObject.transform; t != null; t = t.parent)
			{
				T result = t.GetComponent<T>();
				if(result != null)
					return result;
			}
			
			return null;
		}	
		
		public static T[] GetComponentsInParents<T>(this GameObject gameObject) where T : Component
		{
			List<T> results = new List<T>();
			for(Transform t = gameObject.transform; t != null; t = t.parent)
			{
				T result = t.GetComponent<T>();
				if(result != null)
					results.Add(result);
			}
			
			return results.ToArray();
		}	
		
		public static int GetCollisionMask(this GameObject gameObject, int layer)
		{
			if(layer == -1)
				layer = gameObject.layer;
			
			int mask = 0;
			for(int i = 0; i < 32; i++)
				mask |= (Physics.GetIgnoreLayerCollision(layer, i) ? 0 : 1) << i;
			
			return mask;
		}	
		
        public static T CopyComponent<T>(this T original, GameObject destination) where T : Component
        {
            Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }

        public static Color WithAlpha(this Color color, float alpha)
		{
			return new Color(color.r, color.g, color.b, alpha);
		}

		public static Vector2 CalculateExplosionForce(this Rigidbody2D rb, float expForce, Vector2 expPosition, float expRadius)
		{
			if (expForce == 0 || expRadius == 0) return Vector2.zero;
			Vector2 dir = ((Vector2)(rb.transform.position) - expPosition);
			return dir.normalized * expForce * Mathf.Clamp01(1 - (dir.magnitude / expRadius));
		}

		public static void AddExplosionForce(this Rigidbody2D rb, float expForce, Vector2 expPosition, float expRadius)
        {
			rb.AddForce(rb.CalculateExplosionForce(expForce, expPosition, expRadius));
        }

        public static void LookAt2D(this Transform t, Vector3 worldPosition, float? angleClockwiseOffset = null)
        {
            t.rotation = Quaternion.identity;
            t.Rotate(Vector3.forward, (Mathf.Atan2(t.position.y - worldPosition.y, t.position.x - worldPosition.x) * 180 / Mathf.PI) - 180f - angleClockwiseOffset.GetValueOrDefault());
        }
        public static void LookAt2D(this Transform t, Transform target, float? angleClockwiseOffset = null)
        {
            t.rotation = Quaternion.identity;
            t.Rotate(Vector3.forward, (Mathf.Atan2(t.position.y - target.position.y, t.position.x - target.position.x) * 180 / Mathf.PI) - 180f - angleClockwiseOffset.GetValueOrDefault());
        }
        public static void LookAwayFrom2D(this Transform t, Vector3 worldPosition, float? angleClockwiseOffset = null)
        {
            t.rotation = Quaternion.identity;
            t.Rotate(Vector3.forward, (Mathf.Atan2(t.position.y - worldPosition.y, t.position.x - worldPosition.x) * 180 / Mathf.PI) - angleClockwiseOffset.GetValueOrDefault());
        }
        public static void LookAwayFrom2D(this Transform t, Transform target, float? angleClockwiseOffset = null)
        {
            t.rotation = Quaternion.identity;
            t.Rotate(Vector3.forward, (Mathf.Atan2(t.position.y - target.position.y, t.position.x - target.position.x) * 180 / Mathf.PI) - angleClockwiseOffset.GetValueOrDefault());
        }

        // these need corrections for the different axes calculations
        public static void LookAt2D(this Transform t, Vector3 worldPosition, Vector3 rotationAxis, float? angleClockwiseOffset = null)
        {
            t.rotation = Quaternion.identity;
            t.Rotate(rotationAxis, (Mathf.Atan2(t.position.y - worldPosition.y, t.position.x - worldPosition.x) * 180 / Mathf.PI) - 180f - angleClockwiseOffset.GetValueOrDefault());
        }
        public static void LookAt2D(this Transform t, Transform target, Vector3 rotationAxis, float? angleClockwiseOffset = null)
        {
            t.rotation = Quaternion.identity;
            t.Rotate(rotationAxis, (Mathf.Atan2(t.position.y - target.position.y, t.position.x - target.position.x) * 180 / Mathf.PI) - 180f - angleClockwiseOffset.GetValueOrDefault());
        }
        public static void LookAwayFrom2D(this Transform t, Vector3 worldPosition, Vector3 rotationAxis, float? angleClockwiseOffset = null)
        {
            t.rotation = Quaternion.identity;
            t.Rotate(rotationAxis, (Mathf.Atan2(t.position.y - worldPosition.y, t.position.x - worldPosition.x) * 180 / Mathf.PI) - angleClockwiseOffset.GetValueOrDefault());
        }
        public static void LookAwayFrom2D(this Transform t, Transform target, Vector3 rotationAxis, float? angleClockwiseOffset = null)
        {
            t.rotation = Quaternion.identity;
            t.Rotate(rotationAxis, (Mathf.Atan2(t.position.y - target.position.y, t.position.x - target.position.x) * 180 / Mathf.PI) - angleClockwiseOffset.GetValueOrDefault());
        }

        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);

            float tx = v.x;
            float ty = v.y;

            return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
        }

		public static GameObject Find(this Transform parent, string name, bool includeInactive = false)
		{
			Transform[] trs = parent.GetComponentsInChildren<Transform>(includeInactive);
			foreach (Transform t in trs)
			{
				if (t.name == name)
				{
					return t.gameObject;
				}
			}
			return null;
		}

		public static GameObject FindWithTag(this Transform parent, string tag, bool includeInactive = false)
		{
			Transform[] trs = parent.GetComponentsInChildren<Transform>(includeInactive);
			foreach (Transform t in trs)
			{
				if (t.tag == tag)
				{
					return t.gameObject;
				}
			}
			return null;
		}

		public static IEnumerable<T> Find<T>(this GameObject parent, bool includeInactive = false) where T : Component
		{
			return parent.GetComponentsInChildren<T>(includeInactive);
		}

		public static List<T> FindInactive<T>(this GameObject parent) where T : Component
		{
			List<T> results = new List<T>();
			T[] components = parent.GetComponentsInChildren<T>(true);

			foreach (T component in components)
			{
				if (component.gameObject.activeInHierarchy == false)
				{
					results.Add(component);
				}
			}

			return results;
		}

		public static Transform FindParentWithName(this Transform childTransform, string name)
		{
			Transform t = childTransform;
			while (t.parent != null)
			{
				if (t.parent.name == name)
				{
					return t.parent;
				}
				t = t.parent.transform;
			}
			return null; // Could not find a parent with given tag.
		}

		public static Transform FindParentWithTag(this Transform childTransform, string tag)
		{
			Transform t = childTransform;
			while (t.parent != null)
			{
				if (t.parent.tag == tag)
				{
					return t.parent;
				}
				t = t.parent.transform;
			}
			return null; // Could not find a parent with given tag.
		}

		public static GameObject Find(this Scene scene, string name, IEnumerable<Transform> allTransforms = null)
        {
			if (allTransforms == null)
			{
				allTransforms = DataStructureUtilityMethods.GetAllTransformsInScene(scene);
			}

			foreach(Transform t in allTransforms)
            {
				if (t.name.Equals(name)) return t.gameObject;
            }

			return null; 
        }

		public static List<T> FindObjectsOfType<T>(this Scene scene, IEnumerable<Transform> allTransforms = null) where T : Component
		{
			List<T> results = new List<T>();

			if (allTransforms == null)
			{
				allTransforms = DataStructureUtilityMethods.GetAllTransformsInScene(scene);
			}

			foreach (Transform t in allTransforms)
			{
				T result = t.GetComponent<T>();

				if (result != null) results.Add(result);
			}

			return results;
		}

		/// <summary>
		/// Reset the trail so it can be moved without streaking
		/// </summary>
		public static void Reset(this TrailRenderer trail, MonoBehaviour instance)
		{
			instance.StartCoroutine(ResetTrail(trail));
		}

		/// <summary>
		/// Coroutine to reset a trail renderer trail
		/// </summary>
		/// <param name="trail"></param>
		/// <returns></returns>
		static IEnumerator ResetTrail(TrailRenderer trail)
		{
			var trailTime = trail.time;
			trail.time = 0;
			yield return new WaitForEndOfFrame();
			trail.time = trailTime;
		}
	}
}