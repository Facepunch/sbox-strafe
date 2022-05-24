
using Sandbox;
using Sandbox.UI;
using Strafe.Api;
using Strafe.Leaderboards;

namespace Strafe.UI;

[Hud, UseTemplate]
internal class TabMenu : Panel
{

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

	public void SpawnMyReplay()
	{
		StrafeGame.SpawnMyReplay();
	}

}
