
using Sandbox;

namespace Strafe.Players;

internal partial class StrafePlayer
{
	private bool LightEnabled { get; set; } = false;
	protected virtual Vector3 LightOffset => Vector3.Forward * 10;

	private SpotLightEntity viewLight;

	TimeSince timeSinceLightToggled;

	public void CreateViewModel()
	{
		viewLight = CreateLight();
		viewLight.SetParent( this, "forward_reference", new Transform( LightOffset + Vector3.Up * 12 ) );
		viewLight.EnableViewmodelRendering = true;
		viewLight.Enabled = LightEnabled;

	}
	private SpotLightEntity CreateLight()
	{
		var light = new SpotLightEntity
		{
			Enabled = true,
			DynamicShadows = true,
			Range = 512,
			Falloff = 1.0f,
			LinearAttenuation = 0.0f,
			QuadraticAttenuation = 1.0f,
			Brightness = 3,
			Color = Color.White,
			InnerConeAngle = 20,
			OuterConeAngle = 40,
			FogStength = 1.0f,
			Owner = Owner,
			LightCookie = Texture.Load( "materials/effects/lightcookie.vtex" )
		};

		return light;
	}

}
