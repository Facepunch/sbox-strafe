
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
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

		if( TimeSinceUpdate > 5f )
		{
			Update();
			TimeSinceUpdate = 0f;
		}
	}

	private async void Update()
	{
		var q = await GameServices.Leaderboard.Query( Global.GameIdent, bucket: Global.MapName );

		if ( q?.Entries == null ) return;

		DeleteChildren( true );

		var rank = 1;
		foreach( var entry in q.Entries )
		{
			Add.Label( $"#{rank}   {entry.DisplayName}           {entry.Rating.HumanReadable()}s", q.PlayerPlace.ToString() );
			rank++;
		}
	}

}
