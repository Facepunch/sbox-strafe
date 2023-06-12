
namespace Strafe;

internal static class StrafeClientSettings
{

	public static StrafeSettings Settings { get; private set; }

	static StrafeClientSettings()
	{
		Load();
	}

	public static void Save()
	{
		FileSystem.Data.WriteJson( "strafesettings.json", Settings );
	}

	public static void Load()
	{
		Settings = FileSystem.Data.ReadJsonOrDefault<StrafeSettings>( "strafesettings.json", new() );
	}

	static bool menuWasOpen;
	[GameEvent.Client.Frame]
	static void OnSettingsSaved()
	{
		// todo: this is a hack for in-game to pull latest changes since data in GameMenu isn't shared
		// I might be missing something?
		var menuopen = Game.IsMainMenuVisible;
		if ( menuopen == menuWasOpen ) return;

		menuWasOpen = menuopen;
		Load();
	}

	public static void ResetDefaults()
	{
		Settings = new();
		Save();
	}

	public class StrafeSettings
	{
		public bool ShowInput { get; set; }
		public StrafePlayer.PlayerVisibility PlayerVisibility { get; set; } = StrafePlayer.PlayerVisibility.Fade;
		public ViewModelPositions ViewModelPosition { get; set; } = ViewModelPositions.Right;
	}

}
