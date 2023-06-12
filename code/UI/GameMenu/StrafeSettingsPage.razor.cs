
namespace Strafe.UI;

public partial class StrafeSettingsPage : NavigatorPanel
{

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( firstTime )
			Navigate( "/menu/settings/input" );
	}

}
