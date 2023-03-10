
namespace Strafe.Players;

internal class HandheldViewModel : AnimatedEntity
{

	Vector3 BobPositionOffset;
	TimeSince TimeSinceGrounded;
	TimeSince TimeSinceJumped;

	static Curve GroundedCurve { get; set; }

	public HandheldViewModel()
	{
		var groundedFrames = new List<Curve.Frame>()
		{
			new Curve.Frame() { Value = 0, Time = 0, Out = 20f },
			new Curve.Frame() { Value = 1, Time = 0.5f, Mode = Curve.HandleMode.Linear },
			new Curve.Frame() { Value = 0, Time = 1f }
		};

		GroundedCurve = new Curve( groundedFrames );
	}

	public void Fire()
	{
		CurrentSequence.Name = "fire";

		var attachment = GetAttachment( "muzzle" );
		if( attachment != null )
		{
			var p = Particles.Create( "particles/pistol_muzzleflash.vpcf", attachment.Value.Position );
			p.SetOrientation( 0, Rotation );
		}
	}

	[Event.Client.PostCamera]
	public virtual void PlaceViewmodel()
	{
		if ( Game.IsRunningInVR )
			return;

		Camera.Main.SetViewModelCamera( Camera.FieldOfView );

		Position = Camera.Position + Camera.Rotation.Down * 10 + Camera.Rotation.Right * 8 + Camera.Rotation.Forward * 4;
		Position += BobPositionOffset;
		Rotation = Camera.Rotation * Rotation.From( 10, -15, -6 );

		var groundedA = GroundedCurve.Evaluate( ((float)TimeSinceGrounded).LerpInverse( 0f, .35f ) );
		var jumpedA = GroundedCurve.Evaluate( ((float)TimeSinceJumped).LerpInverse( 0f, .35f ) );

		Rotation *= Rotation.From( groundedA * 5, 0, 0 );
		Rotation *= Rotation.From( jumpedA * -2, 0, 0 );

		if ( Game.LocalPawn is not StrafePlayer pl ) return;

		if( pl.Controller is StrafeController ctrl )
		{
			TimeSinceJumped = ctrl.TimeSinceJumped;
			TimeSinceGrounded = ctrl.TimeSinceGrounded;
		}

		if ( pl.GroundEntity == null )
		{
			BobPositionOffset = BobPositionOffset.LerpTo( Camera.Rotation.Right * 3f, Time.Delta * 10f );
			return;
		}

		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;

		var babStr = pl.Velocity.Length.LerpInverse( 0, 300 );
		var bab = MathF.Sin( 10 * Time.Now ) * babStr * 5;
		var offset = bab * ( left + up * .5f );
		offset += (Vector3)Input.MouseDelta * .01f;

		BobPositionOffset = BobPositionOffset.LerpTo( offset, Time.Delta * 3f );
	}

}
