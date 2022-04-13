
using Sandbox;

namespace Strafe.Players;

internal partial class StrafePlayer : Sandbox.Player
{

	[Net, Predicted]
	public PlayerTimer Timer { get; set; }

	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/citizen/citizen.vmdl" );

		Timer = new()
		{
			Player = this
		};

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
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

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
			Timer.Restart();
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

}
