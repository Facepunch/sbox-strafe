
namespace Strafe;

internal static class StrafeClientSettings
{

	public static StrafeSettings Settings { get; private set; }

	static StrafeClientSettings()
	{
		Settings = Cookie.Get<StrafeSettings>( "strafe_client_settings", new() );
	}

	public static void Save()
	{
		Cookie.Set( "strafe_client_settings", Settings );
	}

	public static void ResetDefaults()
	{
		Settings = new();
		Save();
	}

	public class StrafeSettings
	{
		public bool SnailTrail { get; set; }
		public bool ShowInput { get; set; }
		public StrafePlayer.PlayerVisibility PlayerVisibility { get; set; } = StrafePlayer.PlayerVisibility.Fade;
		public ViewModelPositions ViewModelPosition { get; set; } = ViewModelPositions.Right;
	}

}
