using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NoxCore.GamePatterns
{
	public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T _instance;
		private static object _lock = new object();
		
		public static T Instance
		{
			get
			{
				lock(_lock)
				{
					if(_instance == null)
					{
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<T>();
						singleton.name = typeof(T).ToString();
						DontDestroyOnLoad(singleton);
					}
					return _instance;
				}
			}
		}
	}
}