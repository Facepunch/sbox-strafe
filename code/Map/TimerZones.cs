
using Sandbox;
using Strafe.Players;
using System.Linq;

namespace Strafe.Map;

internal partial class BaseZone : StrafeTrigger 
{ 

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		EnableTouch = true;
		EnableTouchPersists = true;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		var particle = Particles.Create( "particles/gameplay/checkpoint/checkpoint.vpcf" );

		for ( int i = 0; i < 4; i++ )
		{
			var corner = Position + Model.PhysicsBounds.Corners.ElementAt( i );
			corner.z += 1;
			particle.SetPosition( i + 1, corner );
		}
	}

}

[Library( "strafe_linear_start", Description = "Where the timer will start" )]
internal partial class LinearStart : BaseZone
{

}

[Library( "strafe_linear_end", Description = "Where the timer will end" )]
internal partial class LinearEnd : BaseZone
{

}

[Library( "strafe_linear_checkpoint", Description = "Where the timer will set a checkpoint (linear)" )]
internal partial class LinearCheckpoint : BaseZone
{

	[Property]
	public int Checkpoint { get; set; }

}

[Library( "strafe_stage_start", Description = "Where the timer will begin a stage" )]
internal partial class StageStart : BaseZone 
{

	[Property]
	public int Stage { get; set; }

}

[Library( "strafe_stage_end", Description = "Where the timer will end a stage" )]
internal partial class StageEnd : BaseZone
{

	[Property]
	public int Stage { get; set; }

}
