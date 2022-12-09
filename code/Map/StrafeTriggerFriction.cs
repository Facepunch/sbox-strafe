
using Sandbox;
using Editor;
using Strafe.Players;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Strafe.Map;

[Display( Name = "Friction Trigger" )]
[Library( "strafe_trigger_friction", Description = "Set the player's friction level while standing in this zone." ), Category( "Triggers" ), Icon( "flag" )]
[HammerEntity]
internal partial class StrafeTriggerFriction : StrafeTrigger
{

	[Net]
	[Property( "friction_level", Title = "Friction Level" )]
	public Strafe.Players.FrictionLevels FrictionLevel { get; set; } = Strafe.Players.FrictionLevels.Normal;

	public override void SimulatedTouch( StrafeController ctrl )
	{
		base.SimulatedTouch( ctrl );

		ctrl.FrictionLevel = FrictionLevel;
	}

}
