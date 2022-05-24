using Sandbox;
using SandboxEditor;
using Strafe.Players;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Strafe.Map;

[Display( Name = "Friction Trigger" )]
[Library( "strafe_change_friction", Description = "Where the timer will begin a stage" ), Category( "Triggers" ), Icon( "flag" )]
[HammerEntity]
internal partial class StrafeTriggerFriction : StrafeTrigger
{
	[Net]
	[Property( "friction", Title = "Friction" )]
	[MinMax( 0.0f, 30.0f )]
	public float SetFriction { get; set; } = 1f;


	public override void SimulatedStartTouch( StrafeController ctrl )
	{
		base.SimulatedStartTouch( ctrl );
		ctrl.GroundFriction = SetFriction;
	}

	public override void SimulatedEndTouch( StrafeController ctrl )
	{
		base.SimulatedEndTouch( ctrl );
		ctrl.GroundFriction = 4.0f;
	}

}
