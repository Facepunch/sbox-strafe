
using Sandbox;
using SandboxEditor;
using Strafe.Players;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Strafe.Map;

[Library( "strafe_trigger_teleport_progress", Description = "Teleports the player to the last checkpoint they've touched." )]
[Display( Name = "Teleport Progress Trigger" ), Category( "Triggers" ), Icon( "rotate_left" )]
[HammerEntity]
internal partial class StrafeTriggerTeleportProgress : StrafeTrigger
{

	/// <summary>
	/// If set, teleports the entity with an offset depending on where the entity was in the trigger teleport. Think world portals. Place the target entity accordingly.
	/// </summary>
	[Property( "teleport_relative", Title = "Teleport Relatively" )]
	[Net]
	public bool TeleportRelative { get; set; }

	/// <summary>
	/// If set, the teleported entity will not have it's velocity reset to 0.
	/// </summary>
	[Property( "keep_velocity", Title = "Keep Velocity" )]
	[Net]
	public bool KeepVelocity { get; set; }

	/// <summary>
	/// If set, the player's view angles will match the target entity's rotation
	/// </summary>
	[Property( "set_viewangles", Title = "Apply ViewAngles" )]
	[Net]
	public bool SetViewAngles { get; set; }

	[Net]
	public Transform TargetTransform { get; set; }

	public override void SimulatedStartTouch( StrafeController ctrl )
	{
		base.SimulatedStartTouch( ctrl );

		if ( !IsEnabled ) return;
		if ( ctrl.Pawn is not StrafePlayer pl ) return;

		var tx = pl.CurrentStage()?.TeleportTransform();
		if ( tx == null ) return;

		Vector3 offset = Vector3.Zero;
		if ( TeleportRelative )
		{
			offset = ctrl.Position - Position;
		}

		if ( !KeepVelocity ) ctrl.Velocity = Vector3.Zero;

		ctrl.Position = tx.Value.Position;
		ctrl.Rotation = tx.Value.Rotation;
		ctrl.Position += offset;

		if ( SetViewAngles )
		{
			pl.SetViewAngles( tx.Value.Rotation.Angles() );
		}
	}

}
