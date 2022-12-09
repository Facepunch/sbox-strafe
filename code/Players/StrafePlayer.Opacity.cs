
using Sandbox;

using Strafe.Utility;

namespace Strafe.Players;

internal partial class StrafePlayer
{

	public enum PlayerVisibility
	{
		Fade = 0,
		Always = 1,
		Never = 2
	}

	[ConVar.Client( "strafe_player_visibility", Saved = true )]
	public static PlayerVisibility Visibility { get; set; } = PlayerVisibility.Fade;

	[Event.Client.Frame]
	private void UpdateRenderAlpha() => this.SetRenderAlphaRecursive( GetRenderAlpha() );

	private float GetRenderAlpha()
	{
		if ( Visibility == PlayerVisibility.Always ) return 1f;
		if ( Visibility == PlayerVisibility.Never ) return 0;
		if ( Visibility == PlayerVisibility.Fade )
		{
			if ( !Local.Pawn.IsValid() ) return 1f;

			const float MaxRenderDistance = 900f;
			var dist = Local.Pawn.Position.Distance( Position );
			var a = 1f - dist.LerpInverse( MaxRenderDistance, MaxRenderDistance * .1f );
			a = Sandbox.Utility.Easing.EaseOut( a );

			return a.Clamp( 0.3f, 1 );
		}

		return 1f;
	}

}
