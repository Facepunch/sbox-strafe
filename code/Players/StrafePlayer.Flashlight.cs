
using Sandbox;

namespace Strafe.Players;

internal partial class StrafePlayer
{

	private SpotLightEntity Flashlight;

	private void ToggleFlashlight()
	{
		Game.AssertClient();

		if( Flashlight == null )
		{
			Flashlight = new SpotLightEntity
			{
				EnableViewmodelRendering = true,
				Enabled = false,
				DynamicShadows = true,
				Color = Color.White,
				InnerConeAngle = 20,
				OuterConeAngle = 40,
				Range = 1024,
				Owner = Owner,
				LightCookie = Texture.Load( "materials/effects/lightcookie.vtex" )
			};
			var tx = Transform.WithPosition( Vector3.Up * 64 + Vector3.Forward * 20f );
			Flashlight.SetParent( this, null, tx );
		}

		Flashlight.Enabled = !Flashlight.Enabled;
		PlaySound( Flashlight.Enabled ? "flashlight-on" : "flashlight-off" );
	}

	[Event.Client.Frame]
	private void UpdateFlashlight()
	{
		if ( !Flashlight.IsValid() ) return;
		if ( !Flashlight.Enabled ) return;

		Flashlight.Position = EyePosition;
		Flashlight.Rotation = EyeRotation;
	}

}
