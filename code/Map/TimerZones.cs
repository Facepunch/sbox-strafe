﻿
using Sandbox;
using Strafe.Players;
using System.Linq;

namespace Strafe.Map;

internal partial class BaseZone : StrafeTrigger 
{ 

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		EnableTouch = true;
		EnableTouchPersists = true;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		var particle = Particles.Create( "particles/gameplay/checkpoint/checkpoint.vpcf" );

		for ( int i = 0; i < 4; i++ )
		{
			var corner = Position + Model.PhysicsBounds.Corners.ElementAt( i );
			corner.z += 1;
			particle.SetPosition( i + 1, corner );
		}
	}

	public override void SimulatedStartTouch( StrafeController ctrl )
	{
		base.SimulatedStartTouch( ctrl );

		if ( IsServer ) return;

		UI.Chat.AddChatEntry( "", $"Entered {GetType()}" );
	}

	public override void SimulatedEndTouch( StrafeController ctrl )
	{
		base.SimulatedEndTouch( ctrl );

		if ( IsServer ) return;

		UI.Chat.AddChatEntry( "", $"Exited {GetType()}" );
	}

}

[Library( "strafe_linear_start", Description = "Where the timer will start" )]
internal partial class LinearStart : BaseZone
{

	public override void SimulatedEndTouch( StrafeController ctrl )
	{
		base.SimulatedEndTouch( ctrl );

		if ( ctrl.Pawn is not StrafePlayer pl ) return;

		pl.Timer.State = TimerStates.Live;
		pl.Timer.Time = 0;
	}

}

[Library( "strafe_linear_end", Description = "Where the timer will end" )]
internal partial class LinearEnd : BaseZone
{

}

[Library( "strafe_linear_checkpoint", Description = "Where the timer will set a checkpoint (linear)" )]
internal partial class LinearCheckpoint : BaseZone
{

	[Property]
	public int Checkpoint { get; set; }

}

[Library( "strafe_stage_start", Description = "Where the timer will begin a stage" )]
internal partial class StageStart : BaseZone 
{

	[Property]
	public int Stage { get; set; }

}

[Library( "strafe_stage_end", Description = "Where the timer will end a stage" )]
internal partial class StageEnd : BaseZone
{

	[Property]
	public int Stage { get; set; }

}
