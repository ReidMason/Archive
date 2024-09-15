using UnityEngine;
using System.Collections;

namespace NoxCore.Debugs
{
	public class BaseDebugEnum
	{ 
        public static readonly BaseDebugEnum HULL = new BaseDebugEnum( 0 );
		public static readonly BaseDebugEnum SHIELDS = new BaseDebugEnum( 1 );		
		public static readonly BaseDebugEnum ENGINES = new BaseDebugEnum( 2 );		
		public static readonly BaseDebugEnum DEVICES = new BaseDebugEnum( 3 );
		public static readonly BaseDebugEnum MODULES = new BaseDebugEnum( 4 );
		public static readonly BaseDebugEnum WEAPONS = new BaseDebugEnum( 5 );
		public static readonly BaseDebugEnum ORDNANCES = new BaseDebugEnum( 6 );
		public static readonly BaseDebugEnum SQUADRON = new BaseDebugEnum(7);
		public static readonly BaseDebugEnum WING = new BaseDebugEnum(8);
		public static readonly BaseDebugEnum FLEET = new BaseDebugEnum(9);
		//public static readonly BaseDebugEnum CLOAKERS = new BaseDebugEnum( 8 );

		public int InternalValue { get; protected set; }
		
		protected BaseDebugEnum( int internalValue )
		{
			this.InternalValue = internalValue;
		}	
	}
}
