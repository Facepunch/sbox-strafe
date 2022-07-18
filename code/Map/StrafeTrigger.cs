
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

	public Transform SpawnTransform()
	{
		var pos = WorldSpaceBounds.Center;
		var height = WorldSpaceBounds.Size.z;
		var tr = Trace.Ray( pos, pos + Vector3.Down * height * .55f )
			.WorldOnly()
			.Run();

		if ( !tr.Hit )
		{
			return new Transform( WorldSpaceBounds.Center, Rotation, 1 );
		}

		var endpos = tr.EndPosition + Vector3.Up;
		endpos.z = (int)endpos.z;

		return new Transform( endpos, Rotation, 1 );
	}

}
