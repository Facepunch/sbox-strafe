
using Sandbox;
using Strafe.Players;
using System.Linq;

namespace Strafe.Map;

[Library( "strafe_trigger_start", Description = "Where the timer will begin a stage" )]
internal partial class StageStart : BaseZone
{

	[Net, Property]
	public int Stage { get; set; } = 1;

	public bool IsFirstStage => Stage <= 1;


	public override void SimulatedStartTouch( StrafeController ctrl )
	{
		base.SimulatedStartTouch( ctrl );

		if ( ctrl.Pawn is not StrafePlayer pl ) return;

		if ( StrafeGame.Current.CourseType == CourseTypes.Linear )
		{
			pl.Stage( 0 ).SetCurrent();
		}

		if ( StrafeGame.Current.CourseType == CourseTypes.Staged )
		{
			if ( IsFirstStage )
			{
				pl.Stage( 0 ).SetCurrent();
			}

			pl.Stage( Stage ).SetCurrent();
		}
	}

	public override void SimulatedEndTouch( StrafeController ctrl )
	{
		base.SimulatedEndTouch( ctrl );

		if ( ctrl.Pawn is not StrafePlayer pl ) return;

		ctrl.LimitSpeed();

		if ( StrafeGame.Current.CourseType == CourseTypes.Linear )
		{
			pl.Stage( 0 ).Start();
			pl.Stage( 1 )?.Start();
		}

		if( StrafeGame.Current.CourseType == CourseTypes.Staged )
		{
			if ( IsFirstStage )
			{
				pl.Stage( 0 ).Start();
			}

			pl.Stage( Stage ).Start();
		}
	}

}

[Library( "strafe_trigger_end", Description = "Where the timer will end a stage" )]
internal partial class StageEnd : BaseZone
{

	[Net, Property]
	public int Stage { get; set; } = 1;

	public bool IsLastStage => Stage == StrafeGame.Current.StageCount;

	public override void SimulatedStartTouch( StrafeController ctrl )
	{
		base.SimulatedStartTouch( ctrl );

		if ( ctrl.Pawn is not StrafePlayer pl ) return;

		if( StrafeGame.Current.CourseType == CourseTypes.Linear )
		{
			pl.Stage( 0 ).Complete();
		}

		if ( StrafeGame.Current.CourseType == CourseTypes.Staged )
		{
			pl.Stage( Stage ).Complete();

			if ( IsLastStage )
			{
				pl.Stage( 0 ).Complete();
			}
		}
	}

}

[Library( "strafe_trigger_checkpoint", Description = "Where the timer will set a checkpoint" )]
internal partial class LinearCheckpoint : BaseZone
{

	[Net, Property]
	public int Checkpoint { get; set; } = 1;

	public override void SimulatedStartTouch( StrafeController ctrl )
	{
		base.SimulatedStartTouch( ctrl );

		if ( ctrl.Pawn is not StrafePlayer pl ) return;

		if ( StrafeGame.Current.CourseType != CourseTypes.Linear )
			return;

		//pl.Stage( 0 ).SetCheckpoint( Checkpoint );
		pl.Stage( Checkpoint ).Complete();
		pl.Stage( Checkpoint ).SetCurrent();
		pl.Stage( Checkpoint + 1 )?.Start();
	}

}

internal partial class BaseZone : StrafeTrigger
{
	[Property]
	public bool EffectEdge { get; set; } = false;

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

		if ( EffectEdge )
		{
			var particle = Particles.Create( "particles/gameplay/checkpoint/checkpoint.vpcf" );

			for ( int i = 0; i < 4; i++ )
			{
				var corner = Position + Model.PhysicsBounds.Corners.ElementAt( i );
				corner.z += 1;
				particle.SetPosition( i + 1, corner );
			}
		}
	}

}
