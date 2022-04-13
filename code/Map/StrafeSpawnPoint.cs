
using Sandbox;

namespace Strafe.Map;

[Library( "info_player_start" )]
internal partial class StrafeSpawnPoint : SpawnPoint
{

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

}
