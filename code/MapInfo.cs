
namespace Strafe;

internal class MapInfo
{

	public string FullIdent { get; set; }
	public MapTypes Type { get; set; }
	public int Difficulty { get; set; }
	public bool Undefined { get; set; }

	public static MapInfo Get ( string fullIdent )
	{
		var result = new MapInfo();
		result.FullIdent = fullIdent;

		// this should come from the api or map data
		// but it's ok for now to manually go through submitted maps

		switch ( fullIdent )
		{
			case "starblue.bhop_nuke":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = 2;
				break;
			case "starblue.speed_glow":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = 3;
				break;
			case "archlinux.surf_low_effort":
				result.Type = MapTypes.Surf;
				result.Difficulty = 1;
				break;
			case "shoukocorp.surf_canals":
				result.Type = MapTypes.Surf;
				result.Difficulty = 1;
				break;
			case "starblue.f1o_m1":
				result.Type = MapTypes.Quake;
				result.Difficulty = 2;
				break;
			case "desinc.infinisurf":
				result.Type = MapTypes.Surf;
				result.Difficulty = 1;
				break;
			case "guffin.freezer":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = 2;
				break;
			case "gkaf.strafe_edge":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = 2;
				break;
			case "pilgrim.surf_canyon":
				result.Type = MapTypes.Surf;
				result.Difficulty = 2;
				break;
			case "obc.bhop_swooloe":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = 2;
				break;
			case "facepunch.strafe_hackr":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = 2;
				break;
			case "rival.surf_combov2":
				result.Type = MapTypes.Surf;
				result.Difficulty = 2;
				break;
			case "bafkb.bhopaqua":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = 2;
				break;
			default:
				result.Undefined = true;
				result.Difficulty = 0;
				result.Type = MapTypes.Bunnyhop;
				break;
		}

		return result;
	}

}
