
namespace Strafe;

internal class MapInfo
{

	public string FullIdent { get; set; }
	public MapTypes Type { get; set; }
	public MapDifficulties Difficulty { get; set; }
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
				result.Difficulty = MapDifficulties.Moderate;
				break;
			case "starblue.speed_glow":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = MapDifficulties.Challenging;
				break;
			case "archlinux.surf_low_effort":
				result.Type = MapTypes.Surf;
				result.Difficulty = MapDifficulties.Easy;
				break;
			case "shoukocorp.surf_canals":
				result.Type = MapTypes.Surf;
				result.Difficulty = MapDifficulties.Easy;
				break;
			case "starblue.f1o_m1":
				result.Type = MapTypes.Quake;
				result.Difficulty = MapDifficulties.Moderate;
				break;
			case "desinc.infinisurf":
				result.Type = MapTypes.Surf;
				result.Difficulty = MapDifficulties.Easy;
				break;
			case "guffin.freezer":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = MapDifficulties.Moderate;
				break;
			case "gkaf.strafe_edge":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = MapDifficulties.Easy;
				break;
			case "pilgrim.surf_canyon":
				result.Type = MapTypes.Surf;
				result.Difficulty = MapDifficulties.Easy;
				break;
			case "obc.bhop_swooloe":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = MapDifficulties.Moderate;
				break;
			case "facepunch.strafe_hackr":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = MapDifficulties.Moderate;
				break;
			case "rival.surf_combov2":
				result.Type = MapTypes.Surf;
				result.Difficulty = MapDifficulties.Moderate;
				break;
			case "bafkb.bhopaqua":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = MapDifficulties.Moderate;
				break;
			case "bigpog.bhop_easy":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = MapDifficulties.Easy;
				break;
			case "sensta.bhop_hot":
				result.Type = MapTypes.Bunnyhop;
				result.Difficulty = MapDifficulties.Moderate;
				break;
			case "facepunch.procsurfmap":
				result.Type = MapTypes.Surf;
				result.Difficulty = MapDifficulties.Easy;
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
