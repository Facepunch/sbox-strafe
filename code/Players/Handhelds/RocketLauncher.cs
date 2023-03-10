
namespace Strafe.Players;

internal partial class RocketLauncher : Handheld
{

	[Net, Predicted]
	public TimeSince TimeSinceFired { get; set; }

	protected override string ViewModelPath => "models/rocketlauncher/rocketlauncher.vmdl";

	protected override void PrimaryAttack()
	{
		base.PrimaryAttack();

		if ( Owner is not StrafePlayer pl ) return;

		if ( TimeSinceFired < 0.65f ) return;

		TimeSinceFired = 0;

		if ( Game.IsClient )
		{
			ViewModel?.Fire();
		}

		Sound.FromWorld( "sounds/rocketlauncher/rocketlauncher_shoot.sound", pl.EyePosition + pl.EyeRotation.Forward * 10f );

		pl.Rocket = new Rocket()
		{
			Position = pl.EyePosition + pl.EyeRotation.Forward * 10f,
			Rotation = pl.EyeRotation,
		};
	}

}
