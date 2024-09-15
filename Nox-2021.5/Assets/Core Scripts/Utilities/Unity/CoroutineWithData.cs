using UnityEngine;
using System.Collections;

// http://answers.unity3d.com/questions/24640/how-do-i-return-a-value-from-a-coroutine.html

namespace NoxCore.Utilities
{
	public class CoroutineWithData : MonoBehaviour 
	{
		public Coroutine coroutine { get; private set; }
		public object result;
		private IEnumerator target;
		
		public CoroutineWithData(MonoBehaviour owner, IEnumerator target) 
		{
			this.target = target;
			this.coroutine = owner.StartCoroutine(Run());
		}
		
		private IEnumerator Run() 
		{
			while(target.MoveNext()) 
			{
				result = target.Current;
				yield return result;
			}
		}
	}
}

/* Usage

Make a coroutine

 private IEnumerator LoadSomeStuff( ) {
     WWW www = new WWW("http://someurl");
     yield return www;
     if (String.IsNullOrEmpty(www.error) {
         yield return "success";
     } else {
         yield return "fail";
     }
 }

Invoke it and get the return value using the utility class...

 CoroutineWithData cd = new CoroutineWithData(this, LoadSomeStuff( ) );
 yield return cd.coroutine;
 Debug.Log("result is " + cd.result);  //  'success' or 'fail'

*/