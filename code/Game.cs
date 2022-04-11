
using Sandbox;
using Strafe.Players;
using Strafe.UI;

namespace Strafe;

internal partial class StrafeGame : Game
{

	public StrafeGame()
	{
		if ( IsServer )
		{
			Global.TickRate = 66;

			_ = new UIEntity();
		}
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		client.Pawn = new StrafePlayer();
		(client.Pawn as StrafePlayer).Respawn();
	}

}

