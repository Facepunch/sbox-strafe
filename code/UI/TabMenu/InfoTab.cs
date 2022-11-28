
using Sandbox.UI;

namespace Strafe.UI;

[UseTemplate]
[NavigatorTarget("/info")]
internal class InfoTab : Panel
{

	public void Say(string input )
	{
		Chatbox.Say( input );
	}

}
