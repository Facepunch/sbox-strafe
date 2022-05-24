
using Sandbox;
using Sandbox.UI;
using Strafe.Players;

namespace Strafe.UI;

[UseTemplate]
internal class StrafeScoreboard : Panel
{

	public Button StopSpectatingButton { get; set; }

	public override void Tick()
	{
		base.Tick();

		var spectating = Local.Pawn is StrafePlayer pl && pl.SpectateTarget.IsValid();
		StopSpectatingButton.SetClass( "disabled", !spectating );
	}

	public void StopSpectating()
	{
		StrafePlayer.SetSpectateTarget( -1 );
	}

}
