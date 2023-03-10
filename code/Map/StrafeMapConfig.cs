
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

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		Current = this;
	}

}

public enum MapTypes
{
	Surf = 1,
	Bunnyhop = 2,
	RocketJump = 3,
	Quake = 4
}

public enum MapStates
{
	Preview = 1,
	Released = 2
}
