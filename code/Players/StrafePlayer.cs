
using Sandbox;

namespace Strafe.Players;

internal partial class StrafePlayer : Sandbox.Player
{

	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new StrafeController()
		{
			AirAcceleration = 150,
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

}
