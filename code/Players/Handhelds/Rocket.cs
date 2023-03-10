
namespace Strafe.Players;

internal struct Rocket 
{

	public Vector3 Position { get; set; } = 0;
	public Rotation Rotation { get; set; } = Rotation.Identity;
	public float Velocity { get; set; } = 1100;
	public bool Deleted { get; private set; } = false;

	static ModelEntity RocketModel;

	public Rocket()
	{

	}

	public Rocket ControllerSimulate( StrafeController ctrl )
	{
		if ( Deleted ) return this;

		Position += Rotation.Forward * 1100f * Time.Delta;

		if ( Game.IsClient )
		{
			RocketModel ??= new();
			RocketModel.SetModel( "models/rocketlauncher/rocket.vmdl" );

			RocketModel.Position = Position;
			RocketModel.Rotation = Rotation;
		}

		var tr = Trace.Sphere( 5f, Position, Position ).WorldOnly().Run();
		if ( !tr.Hit ) return this;

		var dir = (ctrl.Position - tr.HitPosition).Normal;
		var dist = Vector3.DistanceBetween( tr.HitPosition, ctrl.Position );
		var str = dist.LerpInverse( 600, 0 );
		ctrl.Velocity += dir * str * 600;

		if ( Game.IsClient )
		{
			using var _ = Prediction.Off();

			Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", tr.HitPosition + tr.Normal );
			Sound.FromWorld( "sounds/rocketlauncher/rocketlauncher_explode.sound", tr.HitPosition + tr.Normal );
		}

		Deleted = true;

		return this;
	}

}
