
using Sandbox;
using Sandbox.UI;
using Strafe.Players;

namespace Strafe.UI;

[UseTemplate]
internal class PlayerListEntry : EasyListEntry<StrafePlayer>
{

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		StrafePlayer.SetSpectateTarget( Item.NetworkIdent );
	}

	public override void Tick()
	{
		base.Tick();

		var p = Local.Pawn as StrafePlayer;
		SetClass( "active", p.SpectateTarget == Item );
	}

}
