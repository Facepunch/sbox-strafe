
using Sandbox;
using Sandbox.UI;
using Strafe.Players;

namespace Strafe.UI;

[Hud, UseTemplate]
internal class StrafeScoreboard : Panel
{

	public Button StopSpectatingButton { get; set; }

	public override void Tick()
	{
		base.Tick();

		var spectating = Local.Pawn is StrafePlayer pl && pl.SpectateTarget.IsValid();
		StopSpectatingButton.SetClass( "disabled", !spectating );
	}

	[Event.BuildInput]
	public void OnBuildInput( InputBuilder b )
	{
		SetClass( "open", b.Down( InputButton.Score ) );

		if ( !HasClass( "open" ) )
		{
			RemoveClass( "cursor" );
		}
		else
		{
			if ( Input.Down( InputButton.Attack1 ) || Input.Down( InputButton.Attack2 ) )
			{
				AddClass( "cursor" );
			}
		}
	}

	public void StopSpectating()
	{
		StrafePlayer.SetSpectateTarget( -1 );
	}

}
