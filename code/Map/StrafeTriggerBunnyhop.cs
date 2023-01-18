
using System.ComponentModel.DataAnnotations;

namespace Strafe.Map;

[Display( Name = "Bunnyhop Trigger" )]
[Library( "strafe_trigger_bunnyhop", Description = "Teleport's the player back if they stand on the ground for more than a frame." ), Category( "Triggers" ), Icon( "flag" )]
[HammerEntity]
internal partial class StrafeTriggerBunnyhop : StrafeTrigger
{

	[Net, Property]
	public bool UseLastCheckpoint { get; set; } = true;
	[Net, Property]
	public string TargetEntity { get; set; }

	[Net]
	public Transform TargetTransform { get; set; }

	[Event.Entity.PostSpawn]
	private void OnEntityPostSpawn()
	{
		if ( UseLastCheckpoint ) return;
		var Targetent = FindByName( TargetEntity );
		if ( Targetent.IsValid() )
		{
			TargetTransform = Targetent.Transform;
		}
	}

	public override void SimulatedTouch( StrafeController ctrl )
	{
		base.SimulatedTouch( ctrl );

		if ( ctrl.GroundedTickCount <= 1 ) return;
		if ( ctrl.Pawn is not StrafePlayer pl ) return;

		var tx = TargetTransform;
		if ( UseLastCheckpoint )
		{
			tx = pl.CurrentStage()?.TeleportTransform() ?? TargetTransform;
		}

		ctrl.Position = tx.Position;
		pl.SetViewAngles( tx.Rotation.Angles() );
	}

}
