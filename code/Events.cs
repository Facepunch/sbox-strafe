
using Sandbox;
using Strafe.Players;

namespace Strafe;

internal static class Events
{
	internal static class Timer
	{

		public class OnStageComplete : EventAttribute
		{
			public OnStageComplete() : base( "timer.onstagecomplete" ) { }
			public static void Run( TimerEntity timer )
			{
				Event.Run( "timer.onstagecomplete", timer );
			}
		}

		public class OnStageStart : EventAttribute
		{
			public OnStageStart() : base( "timer.onstagelive" ) { }
			public static void Run( TimerEntity timer )
			{
				Event.Run( "timer.onstagelive", timer );
			}
		}

	}
}

