
using Sandbox;
using Strafe.Map;
using Strafe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Strafe.Players;

partial class StrafeController : WalkController
{

	[Net, Predicted]
	public bool Momentum { get; set; }
	[Net, Predicted]
	public bool Activated { get; set; }
	[Net, Predicted]
	public FrictionLevels FrictionLevel { get; set; }
	[Net, Predicted]
	public bool Noclip { get; set; }
	[Net, Predicted]
	public int GroundedTickCount { get; set; }

	private List<StrafeTrigger> TouchingTriggers = new();
	private Vector3 LastBaseVelocity;
	private float LastLeft;
	private bool LastGrounded;
	private StrafePlayer Player => Pawn as StrafePlayer;

	public StrafeController()
	{
		Duck = new StrafeDuck( this );
	}

	public void OnDeactivate()
	{
		Activated = false;

		TouchingTriggers.Clear();
	}

	public void OnActivate()
	{
		Activated = true;
	}

	private void JustStrafed()
	{
		foreach ( var ent in Pawn.Children )
		{
			if ( ent is not TimerEntity t ) continue;
			if ( t.State != TimerEntity.States.Live ) continue;
			t.Strafes++;
		}
	}

	private void JustGrounded()
	{

	}

	private void JustJumped()
	{
		foreach ( var ent in Pawn.Children )
		{
			if ( ent is not TimerEntity t ) continue;
			if ( t.State != TimerEntity.States.Live ) continue;
			t.Jumps++;
		}
	}

	public override void Simulate()
	{
		if( Noclip )
		{
			NoclipMove();
			return;
		}

		FrictionLevel = FrictionLevels.Normal;

		DoTriggers();

		LastBaseVelocity = BaseVelocity;

		ApplyMomentum();

		BaseSimulate();

		if ( Player.InputDirection.y != 0 )
		{
			if ( MathF.Sign( Player.InputDirection.y ) != MathF.Sign( LastLeft ) )
			{
				JustStrafed();
			}
		}

		if( !LastGrounded && GroundEntity.IsValid() )
		{
			JustGrounded();
		}

		LastLeft = Player.InputDirection.y;
		LastGrounded = GroundEntity.IsValid();

		if( !GroundEntity.IsValid() )
		{
			GroundedTickCount = 0;
		}
		else
		{
			GroundedTickCount++;
		}
	}

	public override void AirMove()
	{
		SurfaceFriction = 1f;

		base.AirMove();
	}

	public override void Move()
	{
		MoveHelper mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn ).WithoutTags( "player" );
		mover.MaxStandableAngle = GroundAngle;

		mover.TryMove( Time.Delta );

		Position = mover.Position;
		Velocity = mover.Velocity;
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

		var pm = TraceBBox( vBumpOrigin, point, mins, maxs, 4.0f );

		if ( FrictionLevel == FrictionLevels.Floating || pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
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

	private int FindTouchingTriggers( List<StrafeTrigger> list )
	{
		list.Clear();

		var pl = Pawn as Player;
		if ( !pl.IsValid() ) return 0;

		var me = new BBox( Position + mins + Vector3.Down, Position + maxs );

		foreach ( var ent in Entity.All )
		{
			if ( ent is not StrafeTrigger t ) 
				continue;

			var closestPoint = t.PhysicsBody.FindClosestPoint( Pawn.Position );
			if ( !me.Contains( closestPoint ) ) continue;

			list.Add( t );
		}

		return list.Count;
	}

	List<StrafeTrigger> TouchBuffer = new( 32 );
	private void DoTriggers()
	{
		FindTouchingTriggers( TouchBuffer );

		// try not to brick too hard yet
		try
		{
			foreach ( var trigger in TouchBuffer )
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
				if ( !TouchBuffer.Contains( trigger ) )
				{
					trigger.SimulatedEndTouch( this );
				}
			}
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}

		TouchingTriggers.Clear();
		TouchingTriggers.AddRange( TouchBuffer );
	}

	private void BaseSimulate()
	{
		var targetz = Duck.IsActive ? 40 : EyeHeight;
		var lerpspd = Duck.IsActive ? 8f : 24f;
		var curz = EyeLocalPosition.z;
		var newz = curz.LerpTo( targetz, Time.Delta * lerpspd );

		EyeLocalPosition = Vector3.Up * (newz * Pawn.Scale);
		UpdateBBox();

		EyeLocalPosition += TraceOffset;
		EyeRotation = Rotation.From( Player.ViewAngles );

		CheckLadder();
		Swimming = Pawn.GetWaterLevel() > 0.6f;

		if ( !Swimming && !IsTouchingLadder )
		{
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

			BaseVelocity = BaseVelocity.WithZ( 0 );
		}

		if ( AutoJump ? Input.Down( InputButton.Jump ) : Input.Pressed( InputButton.Jump ) )
		{
			CheckJumpButton();
		}

		bool bStartOnGround = GroundEntity != null;
		if ( bStartOnGround )
		{
			Velocity = Velocity.WithZ( 0 );

			if ( GroundEntity != null )
			{
				var frictionModifier = FrictionLevel switch
				{
					FrictionLevels.Normal => 1.0f,
					FrictionLevels.Sticky => 2.0f,
					FrictionLevels.Skate => 0.05f,
					_ => 1.0f
				};
				ApplyFriction( GroundFriction * SurfaceFriction * frictionModifier );
			}
		}

		WishVelocity = new Vector3( Player.InputDirection.x, Player.InputDirection.y, 0 );
		var inSpeed = WishVelocity.Length.Clamp( 0, 1 );

		if ( !Swimming )
		{
			WishVelocity *= Player.ViewAngles.WithPitch( 0 ).ToRotation();
		}
		else
		{
			WishVelocity *= Player.ViewAngles.ToRotation();
		}


		if ( !Swimming && !IsTouchingLadder )
		{
			WishVelocity = WishVelocity.WithZ( 0 );
		}

		WishVelocity = WishVelocity.Normal * inSpeed;
		WishVelocity *= GetWishSpeed();

		Duck.PreTick();

		bool bStayOnGround = false;
		if ( Swimming )
		{
			if ( Pawn.GetWaterLevel().AlmostEqual( 0.6f, .05f ) )
				CheckWaterJump();

			WaterMove();
		}
		else if ( IsTouchingLadder )
		{
			LadderMove();
		}
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

	public override void CheckJumpButton()
	{
		if ( Swimming )
		{
			ClearGroundEntity();

			Velocity = Velocity.WithZ( 100 );
			return;
		}

		if ( GroundEntity == null )
			return;

		ClearGroundEntity();

		float flGroundFactor = 1.0f;

		var jumpHeight = Duck.IsActive ? 56 : 52;
		float flMul = MathF.Sqrt( 2f * Gravity * jumpHeight );
		float startz = Velocity.z;

		if ( Duck.IsActive )
		{
			Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );
		}
		else
		{
			Velocity += Vector3.Up * (startz + flMul * flGroundFactor);
		}

		Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		JustJumped();
	}

	bool IsTouchingLadder = false;
	Vector3 LadderNormal;

	public override void CheckLadder()
	{
		var wishvel = new Vector3( Player.InputDirection.x, Player.InputDirection.y, 0 );
		wishvel *= Player.ViewAngles.WithPitch( 0 ).ToRotation();
		wishvel = wishvel.Normal;

		if ( IsTouchingLadder )
		{
			if ( Input.Pressed( InputButton.Jump ) )
			{
				Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;

				return;

			}
			else if ( GroundEntity != null && LadderNormal.Dot( wishvel ) > 0 )
			{
				IsTouchingLadder = false;

				return;
			}
		}

		const float ladderDistance = 1.0f;
		var start = Position;
		Vector3 end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : wishvel) * ladderDistance;

		var pm = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithTag( "ladder" )
					.Ignore( Pawn )
					.Run();

		IsTouchingLadder = false;

		if ( pm.Hit )
		{
			IsTouchingLadder = true;
			LadderNormal = pm.Normal;
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
			float upwardMovememnt = Player.InputDirection.x * (Rotation * Vector3.Forward).z * 2;
			upwardMovememnt = Math.Clamp( upwardMovememnt, 0f, DefaultSpeed );
			wishvel[2] += Player.InputDirection.z + upwardMovememnt;
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

	public override TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.WithoutTags( "player" )
					.Ignore( Pawn )
					.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	private void NoclipMove()
	{
		var speed = Input.Down( InputButton.Run ) ? 1500 : 1000;

		Velocity = new Vector3( Player.InputDirection.x, Player.InputDirection.y, 0 );
		Velocity *= Player.ViewAngles.ToRotation();
		Velocity *= speed;

		Position += Velocity * Time.Delta;
	}

}

