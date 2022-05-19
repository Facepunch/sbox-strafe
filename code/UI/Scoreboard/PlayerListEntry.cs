
using Sandbox.UI;
using Strafe.Players;

namespace Strafe.UI;

[UseTemplate]
internal class PlayerListEntry : EasyListEntry<StrafePlayer>
{

	public PlayerListEntry()
	{
		AddClass( "scoreboard-entry" );
	}

}
