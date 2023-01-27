
using System.ComponentModel.DataAnnotations;

namespace Strafe;

[Library( "strafe_trigger_utility" )]
[Display( Name = "Strafe Utility Trigger" ), Description( "A trigger with helpful outputs" ), Category( "Triggers" ), Icon( "rotate_left" )]
[HammerEntity]
partial class StrafeTriggerUtility : StrafeTrigger
{

	protected Output OnJustGrounded { get; set; }

	[Net, Property]
	public float JustGroundedSpeed { get; set; }

	public override void SimulatedTouch( StrafeController ctrl )
	{
		base.SimulatedTouch( ctrl );

		if ( !ctrl.JustGrounded ) return;
		if ( MathF.Abs( ctrl.FallVelocity.z ) < JustGroundedSpeed ) return;

		OnJustGrounded.Fire( ctrl.Pawn );
	}

}
