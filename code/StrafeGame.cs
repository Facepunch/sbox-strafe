
using Sandbox;
using Strafe.Api;
using Strafe.Leaderboards;
using Strafe.Players;
using Strafe.UI;

namespace Strafe;

internal partial class StrafeGame : Game
{

	public static new StrafeGame Current;

	public StrafeGame()
	{
		Current = this;

		if ( IsServer )
		{
			Global.TickRate = 100;

			_ = new UIEntity();
			_ = new RunSubmitter();
		}
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		client.Pawn = new StrafePlayer();
		(client.Pawn as StrafePlayer).Respawn();

		NetworkClientLogin( client );
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		base.MoveToSpawnpoint( pawn );

		var pos = pawn.Position;
		pos.z = (int)(pos.z + 1);
		pawn.Position = pos;
	}

}

