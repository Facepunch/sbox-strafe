
using Sandbox;
using SandboxEditor;
using Strafe.Players;
using Strafe.Utility;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Strafe.Map;

//[Hammer.AutoApplyMaterial( "materials/editor/push/strafe_trigger_push.vmat" )]
[Library( "strafe_trigger_push" )]
[Display( Name = "Push Trigger" ), Category( "Triggers" ), Icon( "double_arrow" )]
[HammerEntity]
internal partial class StrafeTriggerPush : StrafeTrigger
{

	[Property, Net]
	public bool Once { get; set; }
	[Property, Net]
	public bool TriggerOnJump { get; set; }
	[Property, Net]
	public Vector3 Direction { get; set; }
	[Property, Net]
	public float Speed { get; set; }
	[Property, Net]
	public bool Clamp { get; set; }

	public override void SimulatedStartTouch( StrafeController ctrl )
	{
		base.SimulatedStartTouch( ctrl );

		if ( TriggerOnJump )
		{
			if ( !ctrl.GroundEntity.IsValid() || !Input.Down( InputButton.Jump ) )
				return;

			ctrl.Velocity += GetPushVector( ctrl );
			return;
		}

		if ( !Once ) return;

		ctrl.Velocity += GetPushVector( ctrl );
	}

	public override void SimulatedTouch( StrafeController ctrl )
	{
		base.SimulatedTouch( ctrl );

		if ( Once ) return;

		if ( TriggerOnJump )
		{
			if ( !ctrl.GroundEntity.IsValid() || !Input.Down( InputButton.Jump ) ) 
				return;

			ctrl.Velocity += GetPushVector( ctrl );
			return;
		}

		var vecPush = GetPushVector( ctrl );
		if ( ctrl.Momentum && !ctrl.GroundEntity.IsValid() )
		{
			vecPush += ctrl.BaseVelocity;
		}
		ctrl.BaseVelocity = vecPush;
		ctrl.Momentum = true;
	}

	private Vector3 GetPushVector( StrafeController ctrl )
	{
		var result = Direction.Normal * Speed;
		var tr = ctrl.TraceBBox( Position, Position + Vector3.Down * 4f, 4 );
		if ( !tr.Entity.IsValid() ) return result;
		if ( Vector3.GetAngle( tr.Normal, Vector3.Up ) < 1f ) return result;
		
		return result.Clip( tr.Normal );
	}

}
