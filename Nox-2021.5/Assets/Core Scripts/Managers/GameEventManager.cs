using UnityEngine;
using System.Collections;

/* http://blog.ashwanik.in/2013/08/creating-simple-event-manager-in-unity3d.html */

namespace NoxCore.Managers
{
	public static class GameEventManager
	{
		#region standard game events
		////////////////////////////////////
		/*
			Standard game events
		*/
		////////////////////////////////////
		
		public delegate void MatchStateEventHandler(object sender);
		public static event MatchStateEventHandler EnteringScene;
		public static event MatchStateEventHandler MatchIsWaitingToStart;
		public static event MatchStateEventHandler MatchHasStarted;
		public static event MatchStateEventHandler MatchHasEnded;
		public static event MatchStateEventHandler LeavingScene;
		public static event MatchStateEventHandler AbortedMatch;
		#endregion

        public static void reset()
        {
            EnteringScene = null;
            MatchIsWaitingToStart = null;
            MatchHasStarted = null;
            MatchHasEnded = null;
            LeavingScene = null;
            AbortedMatch = null;
        }

		#region standard game event dispatchers
		////////////////////////////////////
		/*
			Standard game event dispatchers go here
		*/
		////////////////////////////////////
		
		public static void Call_EnteringScene(object sender)
		{
			if (EnteringScene != null)
			{
				EnteringScene(sender);
			}
		}
		
		public static void Call_MatchIsWaitingToStart(object sender)
		{
			if (MatchIsWaitingToStart != null)
			{
				MatchIsWaitingToStart(sender);
			}
		}
		public static void Call_MatchHasStarted(object sender)
		{
			if (MatchHasStarted != null)
			{
				MatchHasStarted(sender);
			}
		}
		
		public static void Call_MatchHasEnded(object sender)
		{
			if (MatchHasEnded != null)
			{
				MatchHasEnded(sender);
			}
		}
		
		public static void Call_LeavingScene(object sender)
		{
			if (LeavingScene != null)
			{
				LeavingScene(sender);
			}
		}
		
		public static void Call_AbortedMatch(object sender)
		{
			if (AbortedMatch != null)
			{
				AbortedMatch(sender);
			}
		}	
		#endregion
	}
}