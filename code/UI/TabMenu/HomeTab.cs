
using Sandbox.UI;

namespace Strafe.UI;

[NavigatorTarget("/")]
[UseTemplate]
internal class HomeTab : Panel
{

	public void SpawnMyReplay()
	{
		StrafeGame.SpawnMyReplay();
	}

}
