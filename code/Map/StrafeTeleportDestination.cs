﻿
using Sandbox;
using Editor;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Strafe.Map;

[Library( "strafe_teleport_destination" )]
[Display( Name = "Teleport Destination" ), Category( "Points" ), Icon( "elderly_woman" )]
[HammerEntity]
internal partial class StrafeTeleportDestination : Entity
{

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

}
