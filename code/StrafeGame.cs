﻿
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
			_ = UpdateCpr();
		}
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new StrafePlayer();
		(cl.Pawn as StrafePlayer).Respawn();

		NetworkClientLogin( cl );

		Chat.AddChatEntry( To.Everyone, "Server", $"{cl.Name} has joined the game", "connect" );
		Chat.AddChatEntry( To.Single( cl ), "Server", "Discord invite at https://strafedb.com", "important" );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		Chat.AddChatEntry( To.Everyone, "Server", $"{cl.Name} has disconnected", "connect" );
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		base.MoveToSpawnpoint( pawn );

		var pos = pawn.Position;
		pos.z = (int)(pos.z + 1);
		pawn.Position = pos;
	}

}

