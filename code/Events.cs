
using Sandbox;
using Strafe.Players;

namespace Strafe;

internal static class Events
{
	internal static class Timer
	{

		public class OnStage : EventAttribute
		{
			public OnStage() : base( "timer.onstage" ) { }
			public static void Run( TimerEntity timer )
			{
				Event.Run( "timer.onstage", timer );
			}
		}

	}
}

