
namespace Strafe.Players;

partial class StrafeController : WalkController
{

	const float STAMINA_MAX = 100f;
	const float STAMINA_COST_JUMP = 25f;
	const float STAMINA_COST_FALL = 20f;
	const float STAMINA_RECOVER_RATE = 19f;

	[Net, Predicted]
	public bool Momentum { get; set; }
	[Net, Predicted]
	public float Stamina { get; set; }
	[Net, Predicted]
	public bool Activated { get; set; }
	[Net, Predicted]
	public FrictionLevels FrictionLevel { get; set; }
	[Net, Predicted]
	public bool Noclip { get; set; }
	[Net, Predicted]
	public int GroundedTickCount { get; set; }

	public bool JustJumped { get; private set; }
	public bool JustGrounded { get; private set; }
	public Vector3 FallVelocity { get; private set; }

	// FOR CLIENT SIDE SHIT ONLY CURRENTLY
	public TimeSince TimeSinceJumped { get; private set; }
	public TimeSince TimeSinceGrounded { get; private set; }
	//

	private List<StrafeTrigger> TouchingTriggers = new();
	private Vector3 LastBaseVelocity;
	private float LastLeft;
	private bool LastGrounded;
	private Vector3 LastVelocity;
	private float LastYaw;
	private BaseStyle StyleController;
	private StrafePlayer Player => Pawn as StrafePlayer;

	public StrafeController()
	{
		Duck = new StrafeDuck( this );
	}

	private void OnJustStrafed()
	{
		foreach ( var ent in Pawn.Children )
		{
			if ( ent is not TimerEntity t ) continue;
			if ( t.State != TimerEntity.States.Live ) continue;
			t.Strafes++;
		}
	}

	private void OnJustGrounded( Vector3 fallVelocity )
	{
		JustGrounded = true;
		FallVelocity = fallVelocity;
		TimeSinceGrounded = 0;
	}

	private void OnJustJumped()
	{
		foreach ( var ent in Pawn.Children )
		{
			if ( ent is not TimerEntity t ) continue;
			if ( t.State != TimerEntity.States.Live ) continue;
			t.Jumps++;
		}

		JustJumped = true;
		TimeSinceJumped = 0;
	}

	public override void BuildInput()
	{
		base.BuildInput();

		StyleController?.BuildInput( this );
	}

	private void SetStyleController()
	{
		if ( !Player.IsValid() ) return;

		if( Player.Style == TimerStyles.Normal && StyleController is not NormalStyle )
		{
			StyleController = new NormalStyle();
		}

		if( Player.Style == TimerStyles.Hsw && StyleController is not BhopHswStyle )
		{
			StyleController = new BhopHswStyle();
		}

		if ( Player.Style == TimerStyles.Stamina && StyleController is not StaminaStyle )
		{
			StyleController = new StaminaStyle();
		}

		if ( Player.Style == TimerStyles.W && StyleController is not WOnlyStyle )
		{
			StyleController = new WOnlyStyle();
		}

		if ( Player.Style == TimerStyles.Sw && StyleController is not SidewaysStyle )
		{
			StyleController = new SidewaysStyle();
		}
	}

	void SimulateRockets()
	{
		if ( Player.Rocket.Deleted ) 
			return;

		Player.Rocket = Player.Rocket.ControllerSimulate( this );
	}

	public override void Simulate()
	{
		Vector3 startVelocity = Velocity;

		if ( Noclip )
		{
			NoclipMove();
			return;
		}

		SimulateRockets();

		if( Stamina > 0 )
		{
			Stamina -= Time.Delta;
			if ( Stamina < 0 ) Stamina = 0;
		}

		SetStyleController();
		StyleController?.Simulate( this );

		FrictionLevel = FrictionLevels.Normal;

		CheckSync();
		DoTriggers();

		JustJumped = false;
		JustGrounded = false;
		LastBaseVelocity = BaseVelocity;

		ApplyMomentum();

		BaseSimulate();

		if ( Player.InputDirection.y != 0 )
		{
			if ( MathF.Sign( Player.InputDirection.y ) != MathF.Sign( LastLeft ) )
			{
				OnJustStrafed();
			}
		}

		if( !LastGrounded && GroundEntity.IsValid() )
		{
			OnJustGrounded( startVelocity );
		}

		if( !GroundEntity.IsValid() )
		{
			GroundedTickCount = 0;
		}
		else
		{
			GroundedTickCount++;
		}

		LastLeft = Player.InputDirection.y;
		LastGrounded = GroundEntity.IsValid();
		LastYaw = Player.ViewAngles.yaw;
		LastVelocity = Velocity;
	}

	private void CheckSync()
	{
		if ( GroundEntity.IsValid() ) return;

		var angleDiff = Player.ViewAngles.yaw - LastYaw;
		if ( angleDiff == 0 ) return;

		var goodSync = false;

		var wishDir = Player.InputDirection.WithZ( 0 );
		wishDir *= Player.ViewAngles.WithPitch( 0 ).ToRotation();
		wishDir = wishDir.WithZ( 0 ).Normal;

		var dot = Vector3.Dot( LastVelocity.WithZ( 0 ), wishDir );
		goodSync = dot < AirControl;

		foreach ( var ent in Pawn.Children )
		{
			if ( ent is not TimerEntity t ) continue;
			if ( t.State != TimerEntity.States.Live ) continue;
			t.TotalSync++;
			if ( goodSync ) t.GoodSync++;
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

		var me = new BBox( Position + mins + Vector3.Down, Position + maxs );

		foreach ( var ent in Entity.All )
		{
			if ( ent is not StrafeTrigger t ) continue;

			var closestPoint = t.PhysicsBody.FindClosestPoint( Position );
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
		var curz = Player.EyeLocalPosition.z;
		var newz = curz.LerpTo( targetz, Time.Delta * lerpspd );

		Player.EyeLocalPosition = Vector3.Up * (newz * Pawn.Scale);
		UpdateBBox();

		Player.EyeLocalPosition += TraceOffset;
		Player.EyeRotation = Rotation.From( Player.ViewAngles );
		Rotation = Rotation.From( Player.ViewAngles.WithPitch( 0 ) );

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

			if ( Stamina > 0 )
			{
				ApplyStamina();
			}

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

	private void ApplyStamina()
	{
		float flRatio = ((STAMINA_MAX - Stamina * STAMINA_RECOVER_RATE)) / STAMINA_MAX;

		// This Goldsrc code was run with variable timesteps and it had framerate dependencies.
		// People looking at Goldsrc for reference are usually 
		// (these days) measuring the stoppage at 60fps or greater, so we need
		// to account for the fact that Goldsrc was applying more stopping power
		// since it applied the slowdown across more frames.
		float flReferenceFrametime = 1.0f / 70.0f;
		float flFrametimeRatio = Time.Delta / flReferenceFrametime;

		flRatio = MathF.Pow( flRatio, flFrametimeRatio );

		var vel = Velocity;
		vel.x *= flRatio;
		vel.y *= flRatio;

		Velocity = vel;
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

		if ( Stamina > 0 )
		{
			float flRatio = (STAMINA_MAX - (Stamina * STAMINA_RECOVER_RATE)) / STAMINA_MAX;
			Velocity = Velocity.WithZ( Velocity.z * flRatio );
		}

		if ( StyleController?.UseStamina ?? false )
		{
			Stamina = STAMINA_COST_JUMP / STAMINA_RECOVER_RATE;
		}

		OnJustJumped();
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

