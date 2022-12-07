
using Sandbox;
using Strafe.UI;
using System.Linq;

namespace Strafe.Players;

internal partial class StrafePlayer : Sandbox.Player
{

	[Net]
	public TimerFrame TimerFrame { get; set; }
	[Net]
	public int TimerStage { get; set; }
	[Net]
	public TimerEntity.States TimerState { get; set; }

	private bool TimersCreated;
	private ClothingContainer Clothing;
	private Nametag Nametag;

	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new StrafeController()
		{
			AirAcceleration = 1500,
			WalkSpeed = 260,
			SprintSpeed = 260,
			DefaultSpeed = 260,
			AutoJump = true,
			Acceleration = 5,
			GroundFriction = 4 //Do this just for safety if player respawns inside friction volume.
		};

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing ??= new();
		Clothing.LoadFromClient( Client );
		Clothing.DressEntity( this );

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

	public override void Simulate( Client cl )
	{
		if ( SpectateTarget.IsValid() ) return;

		base.Simulate( cl );

		TimerFrame = Stage( 0 )?.GrabFrame() ?? default;
		TimerStage = CurrentStage()?.Stage ?? 0;
		TimerState = Stage( 0 )?.State ?? default;

		SimulateAnimatorSounds();

		if ( Controller is StrafeController ctrl )
		{
			if ( ctrl.Activated && GetActiveController() != ctrl )
			{
				ctrl.OnDeactivate();
			}
			else if ( !ctrl.Activated && GetActiveController() == ctrl )
			{
				ctrl.OnActivate();
			}

			if ( ctrl.GroundEntity.IsValid() )
			{
				foreach( var child in Children )
				{
					if ( child is not TimerEntity t ) 
						continue;
					t.EnforceGroundState = false;
				}
			}
		}

		foreach ( var child in Children )
		{
			child.Simulate( cl );
		}

		// HACK:should be setting ButtonToSet back to default in BuildInput
		//		after adding it to this player's input but sometimes the button we
		//		want gets missed in simulate.. so just keep trying right here
		if ( IsClient && ButtonToSet != InputButton.Slot9 )
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

		if ( Input.Pressed( InputButton.Flashlight ) && IsClient )
		{
			ToggleFlashlight();
		}
	}

	public override void FrameSimulate( Client cl )
	{
		//base.FrameSimulate( cl );

		if ( !cl.IsOwnedByLocalClient ) return;

		if( SpectateTarget is ReplayEntity replay )
		{
			Camera.Rotation = replay.Frame.Angles.ToRotation();
			Camera.Position = replay.Frame.Position + Vector3.Up * 64;
			Camera.FirstPersonViewer = replay;
		}
		else if( SpectateTarget is StrafePlayer pl )
		{
			Camera.Rotation = pl.ViewAngles.ToRotation();
			Camera.Position = pl.EyePosition;
			Camera.FirstPersonViewer = pl;
		}
		else
		{
			Camera.Rotation = ViewAngles.ToRotation();
			Camera.Position = EyePosition;
			Camera.FirstPersonViewer = this;
		}

		Camera.FieldOfView = Local.UserPreference.FieldOfView;
		Camera.ZNear = 1f;
		Camera.ZFar = 5000.0f;
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
		base.BuildInput();

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

	public override float FootstepVolume() => Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 260f );
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !IsServer )
			return;

		if ( timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume * 10 );
	}

	private TimeSince TimeSinceGroundedSound = 0f;
	private void SimulateAnimatorSounds()
	{
		if ( !IsClient ) return;

		using var _ = Prediction.Off();

		//if ( Animator.HasEvent( "jump" ) && TimeSinceGroundedSound > .2f )
		//{
		//	Sound.FromEntity( "footstep-concrete", this );
		//}

		//if ( Animator.HasEvent( "grounded" ) )
		//{
		//	Sound.FromEntity( "footstep-concrete-land", this );
		//	TimeSinceGroundedSound = 0f;
		//}
	}

	[ConCmd.Client( "+yaw", CanBeCalledFromServer = false )]
	public static void OnYaw( float spd = 0 )
	{
		if ( Local.Pawn is not StrafePlayer pl ) return;

		pl.YawSpeed = spd;
	}

	[ConCmd.Client( "-yaw", CanBeCalledFromServer = false )]
	public static void OnYawRelease()
	{
		if ( Local.Pawn is not StrafePlayer pl ) return;

		pl.YawSpeed = 0;
	}

}
