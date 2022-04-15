
using Sandbox;
using Sandbox.UI;

namespace Strafe.UI;

[Hud, UseTemplate]
internal class TimerHud : Panel
{

	public int Speedometer => (int)(Local.Pawn?.Velocity.WithZ( 0 ).Length ?? 0);

}
