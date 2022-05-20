
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Strafe.Leaderboards;
using Strafe.Players;
using System;

namespace Strafe.UI;

[Hud, UseTemplate]
internal class SpectatorList : Panel
{

	private int Hash;

	public string Heading { get; set; }
	public Panel Canvas { get; set; }

	[Event.Frame]
	private void OnFrame()
	{
		SetClass( "open", ShouldBeOpen() );

		if ( !HasClass( "open" ) ) return;
		if ( Local.Pawn is not StrafePlayer pl ) return;

		var specTarget = pl.SpectateTarget ?? pl;
		var hash = specTarget.NetworkIdent;
		foreach ( var ent in Entity.All )
		{
			if ( ent is not StrafePlayer pl2 ) continue;
			if ( pl2.SpectateTarget != specTarget ) continue;
			hash = HashCode.Combine( hash, pl2.NetworkIdent );
		}

		if ( hash == Hash ) return;
		Hash = hash;

		Rebuild();
	}

	private void Rebuild()
	{
		Canvas.DeleteChildren( true );

		if ( Local.Pawn is not StrafePlayer pl )
			return;

		var specTarget = pl.SpectateTarget ?? pl;

		Heading = $"Spectating {GetName( specTarget )}";

		foreach ( var ent in Entity.All )
		{
			if ( ent is not StrafePlayer pl2 ) continue;
			if ( pl2.SpectateTarget != specTarget ) continue;
			Canvas.Add.Label( pl2.Client.Name );
		}
	}

	private bool ShouldBeOpen()
	{
		if ( Local.Pawn is not StrafePlayer pl )
			return false;

		var checkfor = pl.SpectateTarget ?? pl;

		foreach ( var ent in Entity.All )
		{
			if ( ent is not StrafePlayer pl2 ) continue;
			if ( pl2.SpectateTarget == checkfor ) return true;
		}

		return false;
	}

	private string GetName( Entity specTarget )
	{
		if ( specTarget is StrafePlayer pl && pl.Client.IsValid() )
			return pl.Client.Name;

		if ( specTarget is ReplayEntity rep )
			return new Friend( rep.PlayerId ).Name + " (replay)";

		return "Unknown";
	}

	protected override void PostTemplateApplied()
	{
		Hash = -123;
	}

}
