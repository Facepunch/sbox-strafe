
using Sandbox;
using Strafe.Players;
using System;
using System.ComponentModel.DataAnnotations;

namespace Strafe.Map;

[Library( "strafe_trigger_push" )]
[Display( Name = "Push Trigger" )]
internal partial class TriggerPush : BaseTrigger
{

	[Property, Net]
	public bool Once { get; set; }
	[Property, Net]
	public Vector3 Direction { get; set; }
	[Property, Net]
	public float Speed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		EnableTouchPersists = true;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not StrafePlayer pl ) return;

		pl.BaseVelocity = GetPushVector( pl );
	}

	public override void Touch( Entity other )
	{
		base.Touch( other );

		if ( other is not StrafePlayer pl ) return;
		if ( Once ) return;

		var vecPush = GetPushVector( pl );
		if ( pl.Momentum && !pl.GroundEntity.IsValid() )
		{
			vecPush += pl.BaseVelocity;
		}
		pl.BaseVelocity = vecPush;
		pl.Momentum = true; // kinda dumb, trying to be consistent with source 1 push
	}

	private Vector3 GetPushVector( StrafePlayer pl )
	{
		var result = Direction.Normal * Speed;
		var ctrl = pl.Controller as StrafeController;
		var tr = ctrl.TraceBBox( Position, Position + Vector3.Down * 4f, 4 );
		if ( !tr.Entity.IsValid() ) return result;

		return ClipVelocity( result, tr.Normal );
	}

	Vector3 ClipVelocity( Vector3 vel, Vector3 norm, float overbounce = 1.0f )
	{
		var backoff = Vector3.Dot( vel, norm ) * overbounce;
		var o = vel - (norm * backoff);

		// garry: I don't totally understand how we could still
		//		  be travelling towards the norm, but the hl2 code
		//		  does another check here, so we're going to too.
		var adjust = Vector3.Dot( o, norm );
		if ( adjust >= 1.0f ) return o;

		adjust = MathF.Min( adjust, -1.0f );
		o -= norm * adjust;

		return o;
	}

}
