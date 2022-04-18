
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
		DeleteChildren();

		var q = await GameServices.Leaderboard.Query( Global.GameIdent, bucket: Global.MapName );

		var rank = 1;
		foreach( var entry in q.Entries )
		{
			var lbl = Add.Label( $"#{rank}   {entry.DisplayName}           {entry.Rating.HumanReadable()}", q.PlayerPlace.ToString() );
			rank++;
		}
	}

}
