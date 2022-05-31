
using Sandbox;
using SandboxEditor;

namespace Strafe.Map;

[Library( "strafe_map_config" )]
[HammerEntity]
[Global( "strafe_map_config" )]
[Title( "Map Config" )]
[Description( "Strafe-specific configurations for your map, you should have one of these" )]
internal partial class StrafeMapConfig : Entity
{

	[Property, Net]
	public MapTypes Type { get; set; } = MapTypes.Surf;
	[Property, Net]
	public MapStates State { get; set; } = MapStates.Preview;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

}

public enum MapTypes
{
	Surf = 1,
	Bunnyhop = 2
}

public enum MapStates
{
	Preview = 1,
	Released = 2
}
