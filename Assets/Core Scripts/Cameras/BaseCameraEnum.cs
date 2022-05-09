using UnityEngine;
using System.Collections;

namespace NoxCore.Cameras
{
	public class BaseCameraEnum
	{
		public static readonly BaseCameraEnum FREE = new BaseCameraEnum( 1 );
		public static readonly BaseCameraEnum TRACK_SELECTED = new BaseCameraEnum( 2 );
		
		public int InternalValue { get; protected set; }
		
		protected BaseCameraEnum( int internalValue )
		{
			this.InternalValue = internalValue;
		}	
	}
}