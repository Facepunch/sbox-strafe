
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
			public static void Run( Strafe.Players.TimerEntity timer )
			{
				Event.Run( "timer.onstagecomplete", timer );
			}
		}

		public class OnStageStart : EventAttribute
		{
			public OnStageStart() : base( "timer.onstagelive" ) { }
			public static void Run( Strafe.Players.TimerEntity timer )
			{
				Event.Run( "timer.onstagelive", timer );
			}
		}

		public class OnStageStop : EventAttribute
		{
			public OnStageStop() : base( "timer.onstagestop" ) { }
			public static void Run( Strafe.Players.TimerEntity timer )
			{
				Event.Run( "timer.onstagestop", timer );
			}
		}

		public class OnReset : EventAttribute
		{
			public OnReset() : base( "timer.onreset" ) { }
			public static void Run( Strafe.Players.TimerEntity timer )
			{
				Event.Run( "timer.onreset", timer );
			}
		}

	}
}

