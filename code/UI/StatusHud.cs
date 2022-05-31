
using Sandbox.UI;

namespace Strafe.UI;

[UseTemplate, Hud]
internal class StatusHud : Panel
{

	public string Connection { get; set; } = "Disconnected";

	public override void Tick()
	{
		base.Tick();

		var connected = StrafeGame.Current.Connected;
		if( HasClass("connected") != connected )
		{
			SetClass( "connected", connected );
			Connection = connected ? "Connected" : "Disconnected";
		}

		if( StrafeGame.Current.MapState == Map.MapStates.Preview )
		{
			SetClass( "preview", true );
		}
	}

}
