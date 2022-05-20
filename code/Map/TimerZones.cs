
using Sandbox;
using SandboxEditor;
using Strafe.Players;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Strafe.Map;

//[Hammer.AutoApplyMaterial( "materials/editor/start/strafe_trigger_start.vmat" )]
[Display( Name = "Stage Start" )]
[Library( "strafe_trigger_start", Description = "Where the timer will begin a stage" ), Category( "Triggers" ), Icon( "flag" )]
[HammerEntity]
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

[Display( Name = "Stage End" )]
//[Hammer.AutoApplyMaterial( "materials/editor/end/strafe_trigger_end.vmat" )]
[Library( "strafe_trigger_end", Description = "Where the timer will end a stage" ), Category( "Triggers" ), Icon( "outlined_flag" )]
[HammerEntity]
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

[Display( Name = "Checkpoint" )]
//[Hammer.AutoApplyMaterial( "materials/editor/checkpoint/strafe_trigger_checkpoint.vmat" )]
[Library( "strafe_trigger_checkpoint", Description = "Where the timer will set a checkpoint" ), Category( "Triggers" ), Icon( "flag_circle" )]
[HammerEntity]
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
