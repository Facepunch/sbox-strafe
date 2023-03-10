
using System.Linq;

namespace Strafe.Players;

internal partial class StrafePlayer : AnimatedEntity
{

	[Net]
	public TimerStyles Style { get; set; }
	[Net]
	public TimerFrame TimerFrame { get; set; }
	[Net]
	public int TimerStage { get; set; }
	[Net]
	public TimerEntity.States TimerState { get; set; }
	[Net]
	public int Credits { get; set; }
	[Net]
	public Handheld Handheld { get; set; }
	[Net, Predicted]
	public Rocket Rocket { get; set; }
	[Net, Predicted]
	public PawnController Controller { get; set; }
	[Net, Predicted]
	public Vector3 EyeLocalPosition { get; set; }
	[Net, Predicted]
	public Rotation EyeLocalRotation { get; set; }
	[ClientInput] 
	public Angles ViewAngles { get; set; }
	[ClientInput] 
	public Vector3 InputDirection { get; set; }

	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	public bool DisplayInput;

	private bool TimersCreated;
	private ClothingContainer Clothing;
	private Nametag Nametag;

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "player" );

		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new StrafeController()
		{
			AirAcceleration = 1500,
			WalkSpeed = 260,
			SprintSpeed = 260,
			DefaultSpeed = 260,
			AutoJump = true,
			Acceleration = 5,
			GroundFriction = 4
		};

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		if ( !TimersCreated )
		{
			TimersCreated = true;

			// 0 = entire course
			// 1, 2, 3 etc = per stage
			for ( int i = 0; i <= StrafeGame.Current.StageCount; i++ )
			{
				new TimerEntity()
				{
					Owner = this,
					Parent = this,
					Stage = i
				};
			}
		}

		GameManager.Current?.MoveToSpawnpoint( this );
	}

	public void LoadClothing( IClient cl )
	{
		Clothing ??= new();
		Clothing.LoadFromClient( cl );
		Clothing.DressEntity( this );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Nametag = new( this );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		Nametag?.Delete();
	}

	public override void Simulate( IClient cl )
	{
		if ( SpectateTarget.IsValid() ) return;

		SimulateAnimator();

		Controller?.Simulate( cl, this );

		TimerFrame = Stage( 0 )?.GrabFrame() ?? default;
		TimerStage = CurrentStage()?.Stage ?? 0;
		TimerState = Stage( 0 )?.State ?? default;

		if ( Controller is StrafeController ctrl )
		{
			if ( ctrl.GroundEntity.IsValid() )
			{
				foreach( var child in Children )
				{
					if ( child is not TimerEntity t ) 
						continue;
					t.EnforceGroundState = false;
				}
			}

			if( Game.IsServer && ( ctrl.JustJumped || ctrl.JustGrounded ) && timeSinceLastFootstep > .1f )
			{
				var tr = ctrl.TraceBBox( Position, Position + Vector3.Down * 20f, 4f );

				if ( !tr.Hit ) return;

				tr.Surface.DoFootstep( this, tr, 0, 6.0f );
				timeSinceLastFootstep = 0f;
			}
		}

		for ( int i = 0; i < Children.Count; i++ )
		{
			if ( !Children[i].IsValid() ) continue;
			Children[i].Simulate( cl );
		}

		// HACK:should be setting ButtonToSet back to default in BuildInput
		//		after adding it to this player's input but sometimes the button we
		//		want gets missed in simulate.. so just keep trying right here
		if ( Game.IsClient && ButtonToSet != InputButton.Slot9 )
		{
			if ( Input.Pressed( ButtonToSet ) )
			{
				ButtonToSet = InputButton.Slot9;
			}
		}

		if ( Input.Pressed( InputButton.Drop ) )
		{
			Restart();
		}

		if ( Input.Pressed( InputButton.Reload ) )
		{
			GoBack();
		}

		if ( Input.Pressed( InputButton.Slot8 ) )
		{
			StrafeGame.Current.DoPlayerNoclip( Client );
		}

		if ( Input.Pressed( InputButton.Flashlight ) && Game.IsClient )
		{
			ToggleFlashlight();
		}
	}

	private void SimulateAnimator()
	{
		var helper = new CitizenAnimationHelper( this );
		helper.WithVelocity( Velocity );
		helper.WithLookAt( EyePosition + EyeRotation.Forward * 500f );

		if( Controller is StrafeController ctrl )
		{
			if ( ctrl.JustJumped )
			{
				helper.TriggerJump();
			}
			helper.IsGrounded = ctrl.GroundEntity.IsValid();
		}
	}

	public override void FrameSimulate( IClient cl )
	{
		if ( !cl.IsOwnedByLocalClient ) return;

		if( SpectateTarget is ReplayEntity replay )
		{
			Camera.Rotation = Rotation.Slerp( Camera.Rotation, replay.EyeRotation, 32f * Time.Delta );
			Camera.Position = replay.Position + Vector3.Up * 64;
			Camera.FirstPersonViewer = replay;
		}
		else if( SpectateTarget is StrafePlayer pl )
		{
			Camera.Rotation = Rotation.Slerp( Camera.Rotation, pl.EyeRotation, 32f * Time.Delta );
			Camera.Position = pl.EyePosition;
			Camera.FirstPersonViewer = pl;
		}
		else
		{
			Camera.Rotation = ViewAngles.ToRotation();
			Camera.Position = EyePosition;
			Camera.FirstPersonViewer = this;
		}

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		Camera.ZNear = 1f;
		Camera.ZFar = 10000.0f;
	}

	[ClientRpc]
	public void SetViewAngles( Angles angles )
	{
		UpdateViewAngle = true;
		UpdatedViewAngle = angles;
	}

	private bool UpdateViewAngle;
	private Angles UpdatedViewAngle;
	private float YawSpeed;
	// Purpose: when typing a command like !r to restart let it run
	//			through simulate to get properly predicted.
	public InputButton ButtonToSet { get; set; } = InputButton.Slot9;
	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;

		var look = Input.AnalogLook;

		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
		{
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;

		Controller?.BuildInput();

		if ( UpdateViewAngle )
		{
			UpdateViewAngle = false;
			ViewAngles = UpdatedViewAngle;
		}

		if ( YawSpeed != 0 )
		{
			ViewAngles = ViewAngles.WithYaw( ViewAngles.yaw + YawSpeed * Time.Delta );
		}

		if ( ButtonToSet == InputButton.Slot9 ) return;

		Input.SetButton( ButtonToSet, true );
	}

	public TimerEntity Stage( int stage )
	{
		return Children.FirstOrDefault( x => x is TimerEntity t && t.Stage == stage ) as TimerEntity;
	}

	public TimerEntity CurrentStage()
	{
		return Children.FirstOrDefault( x => x is TimerEntity t && t.Current ) as TimerEntity;
	}

	public void Restart()
	{
		Velocity = 0;
		BaseVelocity = 0;

		foreach ( var child in Children )
		{
			if ( child is not TimerEntity t || !t.IsValid() ) continue;
			t.Stop();
		}

		Stage( 0 )?.TeleportTo();
	}

	public void GoBack()
	{
		Velocity = 0;
		BaseVelocity = 0;

		CurrentStage()?.TeleportTo();
	}

	TimeSince timeSinceLastFootstep = 0;
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive ) return;
		if ( !Game.IsServer ) return;
		if ( timeSinceLastFootstep < 0.2f ) return;

		volume *= Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 260f );

		timeSinceLastFootstep = 0;

		TraceResult tr = default;
		if( Controller is StrafeController ctrl )
		{
			tr = ctrl.TraceBBox( Position, Position + Vector3.Down * 20f, 4f );
		}
		else
		{
			tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
				.Radius( 1 )
				.Ignore( this )
				.Run();
		}

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume * 10 );
	}

	[ConCmd.Client( "+yaw", CanBeCalledFromServer = false )]
	public static void OnYaw( float spd = 0 )
	{
		if ( Game.LocalPawn is not StrafePlayer pl ) return;

		pl.YawSpeed = spd;
	}

	[ConCmd.Client( "-yaw", CanBeCalledFromServer = false )]
	public static void OnYawRelease()
	{
		if ( Game.LocalPawn is not StrafePlayer pl ) return;

		pl.YawSpeed = 0;
	}

	[ConCmd.Client( "noclip", CanBeCalledFromServer = false )]
	public static void DoNoclip()
	{
		if ( Game.LocalPawn is not StrafePlayer pl ) return;

		pl.ButtonToSet = InputButton.Slot8;
	}

	[ConCmd.Server]
	public static void SetTimerStyle( TimerStyles style )
	{
		if ( ConsoleSystem.Caller?.Pawn is not StrafePlayer pl ) 
			return;

		if( pl.TimerState == TimerEntity.States.Live )
		{
			Chatbox.AddChatEntry( To.Single( pl ), "Server", "Style can't be changed when your timer is live." );
			return;
		}

		pl.Style = style;
	}

}
