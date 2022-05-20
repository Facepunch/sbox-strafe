
using Sandbox;

namespace Strafe.Players;

internal partial class StrafePlayer
{

	[Net]
	public Entity SpectateTarget { get; set; }

	[Event.Tick]
	private void EnsureSpectateTarget()
	{
		if ( !SpectateTarget.IsValid() || SpectateTarget == this ) 
			SpectateTarget = null;
	}

	[ServerCmd]
	public static void SetSpectateTarget( int targetIdent )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) return;
		if ( ConsoleSystem.Caller.Pawn is not StrafePlayer pl ) return;

		var target = Entity.FindByIndex( targetIdent );
		if ( !target.IsValid() ) pl.SpectateTarget = null;
		else pl.SpectateTarget = target;
	}

}
