
using Sandbox;
using Sandbox.UI;

namespace Strafe.UI;

[UseTemplate]
internal class StrafeChatEntry : Panel
{

	public string Name { get; set; }
	public string Message { get; set; }

	public TimeSince TimeSinceCreated = 0;

	public override void Tick()
	{
		base.Tick();

		SetClass( "faded", TimeSinceCreated > 8f );
	}

}
