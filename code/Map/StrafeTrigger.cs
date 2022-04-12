
using Sandbox;
using Strafe.Players;

namespace Strafe.Map;

internal class StrafeTrigger : BaseTrigger
{

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		EnableTouch = true;
		EnableTouchPersists = true;
	}

	public virtual void SimulatedStartTouch( StrafeController ctrl ) { }
	public virtual void SimulatedTouch( StrafeController ctrl ) { }
	public virtual void SimulatedEndTouch( StrafeController ctrl ) { }

}
