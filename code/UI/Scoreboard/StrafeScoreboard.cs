﻿
using Sandbox;
using Sandbox.UI;

namespace Strafe.UI;

[Hud, UseTemplate]
internal class StrafeScoreboard : Panel
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
			if ( Input.Down( InputButton.Attack1 ) )
			{
				AddClass( "cursor" );
			}
		}
	}

}
