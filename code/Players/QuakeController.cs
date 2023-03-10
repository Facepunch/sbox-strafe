
namespace Strafe.Players;

internal partial class QuakeController : StrafeController
{

	[Net, Predicted]
	public Vector3 MoveDirection { get; set; }

	public float AirDecel => 2f;
	public float AirAccel => 2f;
	public float AirMaxSpeed => 7f;
	public float QuakeAirControl => 0.3f;
	public float StrafeMaxSpeed => 1 / .0254f;
	public float StrafeAccel => 50f;
	protected override float JumpForce => 315.0f;

	public override void Simulate()
	{
		if( GroundEntity == null && Input.Pressed( InputButton.Jump ) )
		{
			JumpQueued = true;
		}

		base.Simulate();
	}

	public override void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		var curSpeed = Vector3.Dot( Velocity, wishdir );
		var addspeed = wishspeed - curSpeed;
		if ( addspeed <= 0 ) return;

		var accelSpeed = acceleration * Time.Delta * wishspeed;
		if ( accelSpeed > addspeed )
		{
			accelSpeed = addspeed;
		}

		Velocity += accelSpeed * wishdir.WithZ( 0 );
	}

	public override void AirMove()
	{
		SurfaceFriction = 1.0f;

		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		float accel = 0f;
		float wishspeed2 = wishspeed;
		if( Vector3.Dot(Velocity, wishdir) < 0 )
		{
			accel = AirDecel;
		}
		else
		{
			accel = AirAccel;
		}

		if( Player.InputDirection.x == 0 && Player.InputDirection.y != 0 )
		{
			if ( wishspeed > StrafeMaxSpeed )
				wishspeed = StrafeMaxSpeed;

			accel = StrafeAccel;
		}

		Accelerate( wishdir, wishspeed, 0, accel );

		if( QuakeAirControl > 0 )
		{
			DoAirControl( wishdir, wishspeed2 );
		}

		Velocity += BaseVelocity;
		Move();
		Velocity -= BaseVelocity;
	}

	void DoAirControl( Vector3 wishDir, float wishSpeed )
	{
		if ( MathF.Abs( Player.InputDirection.x ) < .01f || MathF.Abs( wishSpeed ) < .01f )
			return;

		var zSpeed = Velocity.z;
		Velocity = Velocity.WithZ( 0 );

		var speed = Velocity.Length;
		Velocity = Velocity.Normal;

		var dot = Vector3.Dot( Velocity, wishDir );
		var k = 32f;
		k *= QuakeAirControl * dot * dot * Time.Delta;

		if( dot > 0 )
		{
			Velocity *= speed + wishDir * k;
			Velocity = Velocity.Normal;
			MoveDirection = Velocity;
		}

		Velocity *= speed;
		Velocity = Velocity.WithZ( zSpeed );
	}

}
