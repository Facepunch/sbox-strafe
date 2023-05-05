
using Sandbox;
using Editor;
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

	public override void SimulatedStartTouch( StrafeController ctrl )
	{
		if ( TriggerOnJump )
		{
			if ( CanTriggerOnJump( ctrl ) )
				ApplyImpulse( ctrl );
			return;
		}

		if ( !Once ) return;

		ApplyImpulse( ctrl );
	}

	public override void SimulatedTouch( StrafeController ctrl )
	{
		if ( TriggerOnJump )
		{
			if ( CanTriggerOnJump( ctrl ) )
				ApplyImpulse( ctrl );
			return;
		}

		if ( Once ) return;

		ApplyMomentum( ctrl );
	}

	private void ApplyImpulse( StrafeController ctrl )
	{
		ctrl.Velocity += GetPushVector( ctrl );
	}

	private void ApplyMomentum( StrafeController ctrl )
	{
		var vecPush = GetPushVector( ctrl );
		if ( ctrl.Momentum && !ctrl.GroundEntity.IsValid() )
		{
			vecPush += ctrl.BaseVelocity;
		}
		ctrl.BaseVelocity = vecPush;
		ctrl.Momentum = true;
	}

	private bool CanTriggerOnJump( StrafeController ctrl )
	{
		if ( ctrl.GroundEntity == null ) return false;
		if ( !Input.Down( "Jump" ) ) return false;

		return true;
	}

	private Vector3 GetPushVector( StrafeController ctrl )
	{
		var result = Direction.Normal * Speed;
		var tr = ctrl.TraceBBox( Position, Position + Vector3.Down * 4f, 4 );

		if ( !tr.Entity.IsValid() ) return result;
		if ( Vector3.GetAngle( tr.Normal, Vector3.Up ) < 1f ) return result;

		return result.Clip( tr.Normal );
	}

	public static void DrawGizmos( EditorContext ctx )
	{
		// get the position of the target in local space
		var arrowSize = 15.0f;
		var directionProp = ctx.Target.GetProperty( "direction" );
		var direction = directionProp.GetValue<Vector3>( 0 ).Normal;

		direction = Gizmo.Transform.NormalToLocal( direction );

		var endpos = direction * 50f;

		// draw an arrow to it
		Gizmo.Draw.Color = Color.Red.WithAlpha( 0.5f );
		Gizmo.Draw.LineThickness = 2;
		Gizmo.Draw.Line( 0, endpos );
		Gizmo.Draw.SolidCone( endpos - direction * arrowSize, direction * arrowSize, 5.0f );
		Gizmo.Draw.SolidBox( new BBox( 0, 3f ) );
	}

}
