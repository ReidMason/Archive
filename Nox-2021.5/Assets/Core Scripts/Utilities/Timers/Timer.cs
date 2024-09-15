using UnityEngine;
using System;
using System.Diagnostics;
using UnityEngine.UI;

namespace NoxCore.Utilities
{
	public class Timer : MonoBehaviour 
	{
		public bool countdown;
		
		[ShowOnlyAttribute]
		public float maxTime;
		protected StopWatchWrapper timer = new StopWatchWrapper();
		
		[ShowOnlyAttribute]
		public bool running;
		protected Text label;
		protected string textPrefix;
		
		protected float _scaledTimeAccumulator;
		public float scaledTimeAccumulator { get { return _scaledTimeAccumulator; } set { _scaledTimeAccumulator = value; } }
		
		[ShowOnlyAttribute]
		public float accumulatedTime;
		
		void Awake()
		{
			label = GetComponent<Text>();
		}
		
		void Start()
		{		
			if (label != null)
			{
				textPrefix = label.text;
			}
		}
		
		// Update is called once per frame
		void Update () 
		{	
			if (running)
			{
				accumulatedTime = ((float)timer.getOffsetTimeSpan().TotalSeconds);
				
				if (countdown == false)
				{
					if (maxTime > 0 && (timer.ElapsedTimeSpan.TotalSeconds + scaledTimeAccumulator) >= maxTime)
					{
						// set timer to maxTime
						stopTimer();
					}
				}
				else 
				{
					if (maxTime - (timer.ElapsedTimeSpan.TotalSeconds + scaledTimeAccumulator) <= 0)
					{
						//						timer.Reset();
						stopTimer();
					}
				}
			}
			
			if (label != null)
			{
				label.text = textPrefix + formatTimer(getTime(), true);
			}
		}
		
		public bool isRunning()
		{
			return running;
		}
		
		public float getTime()
		{
			return ((float)timer.ElapsedTimeSpan.TotalSeconds) + scaledTimeAccumulator;
		}
		
		public void addScaledTime()
		{
			timer.addTime(scaledTimeAccumulator);
			scaledTimeAccumulator = 0;
		}
		
		public void setTimer(float secs, bool reset)
		{
			timer.setTime(secs, reset);
		}
		
		public string getTimeStr()
		{
			if (countdown == false) 
			{
				return formatTimer ( ((float)timer.ElapsedTimeSpan.TotalSeconds) + scaledTimeAccumulator, true);
			}
			else
			{
				return formatTimer(maxTime - (((float)timer.ElapsedTimeSpan.TotalSeconds) + scaledTimeAccumulator), true);
			}
		}
		
		public void startTimer()
		{
			timer.Start();
			running = true;
		}
		
		public void stopTimer()
		{
			timer.Stop();
			running = false;
		}
		
		public void restartTimer()
		{
			timer.Start();
			running = true;
		}
		
		public void resetTimer()
		{
			timer.Reset();
		}
		
		public static TimeSpan getTimeSpan(float seconds)
		{
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(seconds * 1000);
			
			TimeSpan newTimeSpan = new TimeSpan (timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
			
			return newTimeSpan;
		}
		
		public static string formatTimer(float seconds, bool includedMillis)
		{
            TimeSpan timeSpan = TimeSpan.Zero;

            try
            {
                timeSpan = TimeSpan.FromMilliseconds(seconds * 1000);
            }
            catch (OverflowException oe)
            {
                D.warn("Utilties: {0}", oe.ToString() + "\nAttempted to convert number of seconds: " + seconds);
            }
			
			if (includedMillis == true)
			{
				int fraction = timeSpan.Milliseconds;
				return String.Format ("{0:00}:{1:00}:{2:000}", timeSpan.Minutes, timeSpan.Seconds, fraction);
			}
			else
			{
				return String.Format ("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
			}
		}
		
		public static string formatTimer(StopWatchWrapper timer, bool includedMillis)
		{            
			if (includedMillis == true)
			{
				int fraction = timer.ElapsedTimeSpan.Milliseconds;
				return String.Format ("{0:00}:{1:00}:{2:000}", timer.ElapsedTimeSpan.Minutes, timer.ElapsedTimeSpan.Seconds, fraction);
			}
			else
			{
				return String.Format ("{0:00}:{1:00}", timer.ElapsedTimeSpan.Minutes, timer.ElapsedTimeSpan.Seconds);
			}
		}
	}
}