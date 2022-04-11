
using Hammer;
using Sandbox;
using System.Linq;

namespace Strafe.Map;

public partial class BaseZone : BaseTrigger 
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

[EntityTool("test", "test2")]
[Library( "surf_linear_start", Description = "Where the timer will start" )]
public partial class LinearStart : BaseZone
{

}

[Library( "surf_linear_end", Description = "Where the timer will end" )]
internal partial class LinearEnd : BaseZone
{

}

[Library( "surf_linear_checkpoint", Description = "Where the timer will set a checkpoint (linear)" )]
internal partial class LinearCheckpoint : BaseZone
{

}

[Library( "surf_stage_start", Description = "Where the timer will begin a stage" )]
internal partial class StageStart : BaseZone 
{

}

[Library( "surf_stage_end", Description = "Where the timer will end a stage" )]
internal partial class StageEnd : BaseZone
{

}
