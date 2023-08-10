
using Editor;
using Sandbox;
using System.Linq;

namespace Strafe.Map;

[Library( "strafe_map_config" )]
[HammerEntity]
[Global( "strafe_map_config" )]
[Title( "Map Config" )]
[Description( "Strafe-specific configurations for your map, you should have one of these" )]
internal partial class StrafeMapConfig : Entity
{

	public static StrafeMapConfig Current;

	[Property, Net]
	public MapTypes Type { get; set; } = MapTypes.Surf;
	[Property, Net]
	public MapStates State { get; set; } = MapStates.Preview;
	/// <summary>
	/// If unset, allows the player to maintain speed when exiting
	/// start zones.  The first stage is always clamped regardless
	/// of this value.
	/// </summary>
	[Property, Net]
	public bool ClampStartSpeed { get; set; } = true;

	/// <summary>
	/// If true, this map is just an empty arena to spawn geometry in procedurally.
	/// </summary>
	[Property, Net]
	public bool ProceduralArena { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		Current = this;
	}

}

public enum MapTypes
{
	None = 0,
	Surf = 1,
	Bunnyhop = 2,
	RocketJump = 3,
	Quake = 4
}

public enum MapDifficulties
{
	None = 0,
	Easy = 1,
	Moderate = 2,
	Challenging = 3,
	Hard = 4,
	Intense = 5,
	Extreme = 6
}

public enum MapStates
{
	Preview = 1,
	Released = 2
}
