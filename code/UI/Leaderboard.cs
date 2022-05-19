
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Strafe.Api;
using Strafe.Utility;

namespace Strafe.UI;

[Hud, UseTemplate]
internal class Leaderboard : Panel
{

	private TimeSince TimeSinceUpdate;

	public Leaderboard()
	{
		TimeSinceUpdate = 1000;
	}

	public override void Tick()
	{
		base.Tick();

		if( TimeSinceUpdate > 60f )
		{
			Update();
			TimeSinceUpdate = 0f;
		}
	}

	private async void Update()
	{
		var qq = await Backend.FetchPersonalBests( Global.MapName, 0, 10, 0 );

		if ( qq?.Count == null ) return;

		DeleteChildren( true );

		foreach ( var entry in qq )
		{
			Add.Label( $"#{entry.Rank}   {entry.PlayerName}           {entry.Time.ToTime()}" );
		}
	}

}
