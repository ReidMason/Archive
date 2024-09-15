using UnityEngine;
using System;
using System.Collections.Generic;

namespace NoxCore.Placeables
{
	public class Background : MonoBehaviour
	{
        public List<Material> backgrounds = new List<Material>();
		protected MeshRenderer bkgRenderer;
	
		void Awake()
		{
			bkgRenderer = gameObject.GetComponent<MeshRenderer>();
		}

        void Start()
        {
            setBackground(backgrounds[UnityEngine.Random.Range(0, backgrounds.Count)]);
        }

        public void setBackground(string resourcePath)
		{
			Material material = Resources.Load("Materials/Placeables/Backgrounds/" + resourcePath, typeof(Material)) as Material;
			
			if (material != null)
			{
				bkgRenderer.material = material;
			}
		}
		
		public void setBackground(Material material)
		{
			if (material != null)
			{
				bkgRenderer.material = material;
			}
		}
	}
}