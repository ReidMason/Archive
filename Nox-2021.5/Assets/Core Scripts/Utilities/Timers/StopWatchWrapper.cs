using UnityEngine;
using System;
using System.Diagnostics;

namespace NoxCore.Utilities
{
	public class StopWatchWrapper
	{	
		protected TimeSpan _offsetTimeSpan;
		public TimeSpan offsetTimeSpan { get { return _offsetTimeSpan; } set { _offsetTimeSpan = value; } }
		
		protected Stopwatch _stopwatch;
		public Stopwatch stopWatch { get { return _stopwatch; } }
		
		public TimeSpan Elapsed { get { return _stopwatch.Elapsed + _offsetTimeSpan; } }
		//alpaca
		
		public StopWatchWrapper()		
		{		
			_offsetTimeSpan = new TimeSpan(0,0,0);
			_stopwatch = new Stopwatch();		
		}
		
		public StopWatchWrapper(TimeSpan offsetElapsedTimeSpan)		
		{	
			setTime(offsetElapsedTimeSpan, true);
		}
		
		public TimeSpan getOffsetTimeSpan()
		{
			return offsetTimeSpan;
		}
		
		public void setTime(float seconds, bool reset)		
		{				
			_offsetTimeSpan = TimeSpan.FromMilliseconds(seconds * 1000);
			
			if (reset == true)
			{
				_stopwatch = new Stopwatch();
			}
		}
		
		public void setTime(TimeSpan offsetElapsedTimeSpan, bool reset)		
		{		
			_offsetTimeSpan = offsetElapsedTimeSpan;
			
			if (reset == true)
			{
				_stopwatch = new Stopwatch();
			}		
		}
		
		public void addTime(float secs)
		{
			TimeSpan extra = TimeSpan.FromSeconds(secs);
			_offsetTimeSpan += extra;
		}
		
		public void Start()
		{		
			_stopwatch.Start();
		}
		
		public void Stop()		
		{
			_stopwatch.Stop();
		}
		
		public void Reset()
		{
			_stopwatch.Reset();
		}
		
		public TimeSpan ElapsedTimeSpan
		{
			get	
			{	
				if (_offsetTimeSpan.TotalMilliseconds == 0)
				{
					return _stopwatch.Elapsed;
				}
				
				return _stopwatch.Elapsed + _offsetTimeSpan;
			}
			
			set			
			{
				_offsetTimeSpan = value;
			}		
		}	
	}
}