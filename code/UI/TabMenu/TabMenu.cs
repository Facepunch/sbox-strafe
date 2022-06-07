
using Sandbox;
using Sandbox.UI;

namespace Strafe.UI;

[Hud, UseTemplate]
internal class TabMenu : NavigatorPanel
{

	public TabMenu()
	{
		Navigate( "/" );
	}

	[Event.BuildInput]
	public void OnBuildInput( InputBuilder b )
	{
		SetClass( "open", b.Down( InputButton.Score ) );

		if ( !HasClass( "open" ) )
		{
			RemoveClass( "cursor" );
		}
		else
		{
			if ( Input.Down( InputButton.PrimaryAttack ) || Input.Down( InputButton.SecondaryAttack ) )
			{
				AddClass( "cursor" );
			}
		}
	}

}
