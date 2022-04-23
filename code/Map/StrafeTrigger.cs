
using Sandbox;
using Strafe.Players;

namespace Strafe.Map;

internal partial class StrafeTrigger : BaseTrigger
{

	[ConVar.Replicated]
	public static bool strafe_disable_triggers { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		EnableTouch = true;
		EnableTouchPersists = true;
		EnableDrawing = false;
	}

	[Net]
	public bool IsEnabled { get; set; }

	[Event.Tick.Server]
	public void OnTick()
	{
		IsEnabled = Enabled && !strafe_disable_triggers;
	}

	public virtual void SimulatedStartTouch( StrafeController ctrl ) { }
	public virtual void SimulatedTouch( StrafeController ctrl ) { }
	public virtual void SimulatedEndTouch( StrafeController ctrl ) { }

}
