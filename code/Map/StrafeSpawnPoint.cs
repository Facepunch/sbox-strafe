
using Sandbox;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Strafe.Map;

[Hammer.Skip]
[Library( "info_player_start" )]
[Display( Name = "Player Spawn" ), Category( "Spawn" ), Icon( "elderly_woman" )]
internal partial class StrafeSpawnPoint : SpawnPoint
{

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

}
