using UnityEngine;
using System.Collections;

namespace NoxCore.Cameras
{
	public class RaceCameraEnum : BaseCameraEnum
	{
		public static readonly RaceCameraEnum TRACK_RACE_LEADER = new RaceCameraEnum( 3 );
		public static readonly RaceCameraEnum TRACK_LAST_RACER = new RaceCameraEnum( 4 );
		public static readonly RaceCameraEnum FOLLOW_PREVIOUS_RACER = new RaceCameraEnum( 5 );
		public static readonly RaceCameraEnum FOLLOW_NEXT_RACER = new RaceCameraEnum( 6 );
				
		protected RaceCameraEnum( int internalValue ) : base(internalValue) 
		{
			this.InternalValue = internalValue;
		}				
	}
}