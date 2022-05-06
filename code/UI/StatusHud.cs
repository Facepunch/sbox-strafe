
using Sandbox.UI;
using Strafe.Api;

namespace Strafe.UI;

[UseTemplate, Hud]
internal class StatusHud : Panel
{

	public string Connection { get; set; } = "Disconnected";

	public override void Tick()
	{
		base.Tick();

		var connected = StrafeApi.Connected;
		if( HasClass("connected") != connected )
		{
			SetClass( "connected", connected );
			Connection = connected ? "Connected" : "Disconnected";
		}
	}

}
