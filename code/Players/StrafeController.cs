
using Sandbox;
using Strafe.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Strafe.Players;

partial class StrafeController : WalkController
{

	[Net, Predicted]
	public bool Momentum { get; set; }
	[Net, Predicted]
	public bool Activated { get; set; }

	private List<StrafeTrigger> TouchingTriggers = new();
	private Vector3 LastBaseVelocity;
	private float LastLeft;

	public void OnDeactivate()
	{
		Activated = false;

		TouchingTriggers.Clear();
	}

	public void OnActivate()
	{
		Activated = true;
	}

	public override void Simulate()
	{
		DoTriggers();

		LastBaseVelocity = BaseVelocity;

		ApplyMomentum();

		BaseSimulate();

		if ( Input.Left != 0 )
		{
			if ( MathF.Sign( Input.Left ) != MathF.Sign( LastLeft ) )
				AddEvent( "strafe" );
		}

		LastLeft = Input.Left;
	}

	public override void OnEvent( string name )
	{
		base.OnEvent( name );

		if ( name.Equals( "jump" ) )
		{
			foreach ( var ent in Pawn.Children )
			{
				if ( ent is not TimerEntity t ) continue;
				if ( t.State != TimerEntity.States.Live ) continue;
				t.Jumps++;
			}
		}

		if ( name.Equals( "strafe" ) )
		{
			foreach ( var ent in Pawn.Children )
			{
				if ( ent is not TimerEntity t ) continue;
				if ( t.State != TimerEntity.States.Live ) continue;
				t.Strafes++;
			}
		}
	}

	public override void AirMove()
	{
		SurfaceFriction = 1f;

		base.AirMove();
	}

	public override void CategorizePosition( bool bStayOnGround )
	{
		SurfaceFriction = 1.0f;

		// Doing this before we move may introduce a potential latency in water detection, but
		// doing it after can get us stuck on the bottom in water if the amount we move up
		// is less than the 1 pixel 'threshold' we're about to snap to.	Also, we'll call
		// this several times per frame, so we really need to avoid sticking to the bottom of
		// water on each call, and the converse case will correct itself if called twice.
		//CheckWater();

		var point = Position - Vector3.Up * 2;
		var vBumpOrigin = Position;

		//
		//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
		//
		bool bMovingUpRapidly = Velocity.z + LastBaseVelocity.z > MaxNonJumpVelocity;
		bool bMovingUp = Velocity.z + LastBaseVelocity.z > 0;

		bool bMoveToEndPos = false;

		if ( GroundEntity != null ) // and not underwater
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}
		else if ( bStayOnGround )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}

		if ( bMovingUpRapidly || Swimming ) // or ladder and moving up
		{
			ClearGroundEntity();
			return;
		}

		var pm = TraceBBox( vBumpOrigin, point, 4.0f );

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundEntity();
			bMoveToEndPos = false;

			if ( Velocity.z + LastBaseVelocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}

		if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
		{
			Position = pm.EndPosition;
		}
	}

	public void LimitSpeed()
	{
		var prevz = Velocity.z;
		BaseVelocity = 0;
		Velocity = Velocity.WithZ( 0 ).ClampLength( 290 );
		Velocity = Velocity.WithZ( prevz );
	}

	private void ApplyMomentum()
	{
		if ( !Momentum )
		{
			Velocity += (1.0f + (Time.Delta * 0.5f)) * BaseVelocity;
			BaseVelocity = Vector3.Zero;
		}

		Momentum = false;
	}

	private List<StrafeTrigger> FindTouchingTriggers()
	{
		var result = new List<StrafeTrigger>();
		var pl = Pawn as Player;
		if ( !pl.IsValid() ) return result;

		foreach ( var ent in Entity.All.OfType<StrafeTrigger>() )
		{
			var bbox = ent.PhysicsBody.GetBounds();

			var me = new BBox( Position + mins, Position + maxs );
			if ( !bbox.Overlaps( me ) )
				continue;

			result.Add( ent );
		}

		return result;
	}

	private void DoTriggers()
	{
		var touchingNow = FindTouchingTriggers();

		// try not to brick too hard yet
		try
		{
			foreach ( var trigger in touchingNow )
			{
				if ( !TouchingTriggers.Contains( trigger ) )
				{
					trigger.SimulatedStartTouch( this );
				}
				else
				{
					trigger.SimulatedTouch( this );
				}
			}

			foreach ( var trigger in TouchingTriggers )
			{
				if ( !touchingNow.Contains( trigger ) )
				{
					trigger.SimulatedEndTouch( this );
				}
			}
		}
		catch ( System.Exception e )
		{
			Log.Error( e );
		}

		TouchingTriggers.Clear();
		TouchingTriggers.AddRange( touchingNow );
	}

	private void BaseSimulate()
	{
		EyeLocalPosition = Vector3.Up * (EyeHeight * Pawn.Scale);
		UpdateBBox();

		EyeLocalPosition += TraceOffset;
		EyeRotation = Input.Rotation;

		CheckLadder();
		Swimming = Pawn.WaterLevel > 0.6f;

		if ( !Swimming /*&& !IsTouchingLadder */)
		{
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

			BaseVelocity = BaseVelocity.WithZ( 0 );
		}

		if ( !Swimming )
		{
			if ( AutoJump ? Input.Down( InputButton.Jump ) : Input.Pressed( InputButton.Jump ) )
			{
				CheckJumpButton();
			}
		}

		bool bStartOnGround = GroundEntity != null;
		if ( bStartOnGround )
		{
			Velocity = Velocity.WithZ( 0 );

			if ( GroundEntity != null )
			{
				ApplyFriction( GroundFriction * SurfaceFriction );
			}
		}

		WishVelocity = new Vector3( Input.Forward, Input.Left, 0 );
		var inSpeed = WishVelocity.Length.Clamp( 0, 1 );

		if ( !Swimming )
		{
			WishVelocity *= Input.Rotation.Angles().WithPitch( 0 ).ToRotation();
		}
		else
		{
			WishVelocity *= Input.Rotation.Angles().ToRotation();
		}


		if ( !Swimming/* && !IsTouchingLadder*/ )
		{
			WishVelocity = WishVelocity.WithZ( 0 );
		}

		WishVelocity = WishVelocity.Normal * inSpeed;
		WishVelocity *= GetWishSpeed();

		Duck.PreTick();

		bool bStayOnGround = false;
		if ( Swimming )
		{
			if ( Pawn.WaterLevel.AlmostEqual( 0.6f, .05f ) )
				CheckWaterJump();

			ApplyFriction( 1f );
			WaterMove();
		}
		//else if ( IsTouchingLadder )
		//{
		//	LadderMove();
		//}
		else if ( GroundEntity != null )
		{
			bStayOnGround = true;
			WalkMove();
		}
		else
		{
			AirMove();
		}

		CategorizePosition( bStayOnGround );

		// FinishGravity
		if ( !Swimming/* && !IsTouchingLadder*/ )
		{
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		}

		if ( GroundEntity != null )
		{
			Velocity = Velocity.WithZ( 0 );
		}
	}

	[Net, Predicted]
	public TimeSince TimeSinceWaterJump { get; set; }
	private void CheckWaterJump()
	{
		if ( !Input.Down( InputButton.Jump ) )
			return;

		if ( TimeSinceWaterJump < 2f )
			return;

		if ( Velocity.z < -180 )
			return;

		var fwd = Rotation * Vector3.Forward;
		var flatvelocity = Velocity.WithZ( 0 );
		var curspeed = flatvelocity.Length;
		flatvelocity = flatvelocity.Normal;
		var flatforward = fwd.WithZ( 0 ).Normal;

		// Are we backing into water from steps or something?  If so, don't pop forward
		if ( !curspeed.AlmostEqual( 0f ) && (Vector3.Dot( flatvelocity, flatforward ) < 0f) )
			return;

		var vecstart = Position + (mins + maxs) * .5f;
		var vecend = vecstart + flatforward * 24f;

		var tr = TraceBBox( vecstart, vecend, 0f );
		if ( tr.Fraction < 1.0f )
		{
			const float WATERJUMP_HEIGHT = 8f;
			vecstart.z += Position.z + EyeHeight + WATERJUMP_HEIGHT;

			vecend = vecstart + flatforward * 24f;

			tr = TraceBBox( vecstart, vecend );
			if ( tr.Fraction == 1.0f )
			{
				vecstart = vecend;
				vecend.z -= 1024f;
				tr = TraceBBox( vecstart, vecend );
				if ( (tr.Fraction < 1.0f) && (tr.Normal.z >= 0.7f) )
				{
					Velocity = Velocity.WithZ( 356f );
					TimeSinceWaterJump = 0f;
				}
			}
		}
	}

	public override void WaterMove()
	{
		var wishvel = WishVelocity;

		if ( Input.Down( InputButton.Jump ) )
		{
			wishvel[2] += DefaultSpeed;
		}
		else if ( wishvel.IsNearlyZero() )
		{
			wishvel[2] -= 60;
		}
		else
		{
			float upwardMovememnt = Input.Forward * (Rotation * Vector3.Forward).z * 2;
			upwardMovememnt = Math.Clamp( upwardMovememnt, 0f, DefaultSpeed );
			wishvel[2] += Input.Up + upwardMovememnt;
		}

		var speed = Velocity.Length;
		var wishspeed = Math.Min( wishvel.Length, DefaultSpeed ) * 0.8f;
		var wishdir = wishvel.Normal;

		if ( speed > 0 )
		{
			var newspeed = speed - Time.Delta * speed * GroundFriction * SurfaceFriction;
			if ( newspeed < 0.1f )
			{
				newspeed = 0;
			}

			Velocity *= newspeed / speed;
		}

		if ( wishspeed >= 0.1f )  // old !
		{
			Accelerate( wishdir, wishspeed, 100, Acceleration );
		}

		Velocity += BaseVelocity;

		Move();

		Velocity -= BaseVelocity;
	}

}

