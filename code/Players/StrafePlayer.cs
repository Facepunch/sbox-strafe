
using Sandbox;
using System.Linq;

namespace Strafe.Players;

internal partial class StrafePlayer : Sandbox.Player
{

	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new StrafeController()
		{
			AirAcceleration = 1500,
			WalkSpeed = 260,
			SprintSpeed = 260,
			DefaultSpeed = 260,
			AutoJump = true,
			Acceleration = 5,
		};

		CameraMode = new StrafeCamera();
		Animator = new StandardPlayerAnimator();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		// 0 = entire course
		// 1, 2, 3 etc = per stage
		for( int i = 0; i <= StrafeGame.Current.StageCount; i++ )
		{
			new TimerEntity()
			{
				Owner = this,
				Parent = this,
				Stage = i
			};
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if( Controller is StrafeController ctrl ) 
		{
			if( ctrl.Activated && GetActiveController() != ctrl )
			{
				ctrl.OnDeactivate();
			}
			else if( !ctrl.Activated && GetActiveController() == ctrl )
			{
				ctrl.OnActivate();
			}
		}

		foreach( var child in Children )
		{
			child.Simulate( cl );
		}

		// HACK:should be setting ButtonToSet back to default in BuildInput
		//		after adding it to this player's input but sometimes the button we
		//		want gets missed in simulate.. so just keep trying right here
		if ( IsClient && ButtonToSet != InputButton.Slot9 )
		{
			if ( Input.Pressed( ButtonToSet ) )
			{
				ButtonToSet = InputButton.Slot9;
			}
		}
		
		if ( Input.Pressed( InputButton.Reload ) )
		{
			Restart();
		}

		if ( Input.Pressed( InputButton.Drop ) )
		{
			GoBack();
		}
	}

	// Purpose: when typing a command like !r to restart let it run
	//			through simulate to get properly predicted.
	public InputButton ButtonToSet { get; set; } = InputButton.Slot9;
	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		if ( ButtonToSet == InputButton.Slot9 ) return;

		input.SetButton( ButtonToSet, true );
	}

	public TimerEntity Stage( int stage )
	{
		return Children.First( x => x is TimerEntity t && t.Stage == stage ) as TimerEntity;
	}

	public TimerEntity CurrentStage()
	{
		return Children.FirstOrDefault( x => x is TimerEntity t && t.Current ) as TimerEntity;
	}

	public void Restart()
	{
		Velocity = 0;
		BaseVelocity = 0;

		Children.OfType<TimerEntity>().ToList().ForEach( x => x.Stop() );

		Stage( 1 ).TeleportTo();
	}

	public void GoBack()
	{
		Velocity = 0;
		BaseVelocity = 0;

		CurrentStage()?.TeleportTo();
	}

}
