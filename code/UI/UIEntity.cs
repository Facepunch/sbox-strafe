
using Sandbox;
using Sandbox.UI;

namespace Strafe.UI;

internal class UIEntity : HudEntity<RootPanel>
{

	public UIEntity()
	{
		if ( IsServer ) return;

		RootPanel.AddChild<Hud>();
	}

}
