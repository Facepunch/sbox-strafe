
using Sandbox;
using Sandbox.UI;

namespace Strafe.UI;

internal class UIEntity : HudEntity<RootPanel>
{

	public UIEntity()
	{
		if ( IsServer ) return;

		RootPanel.StyleSheet.Load( "UI/Styles/_styles.scss" );

		Rebuild();
	}

	private void Rebuild()
	{
		RootPanel.DeleteChildren();

		var hudElements = Library.GetAttributes<HudAttribute>();
		foreach ( var element in hudElements )
		{
			var instance = element.Create<Panel>();
			if ( instance == null ) continue;
			RootPanel.AddChild( instance );
		}
	}

	[Event.Hotload]
	private void OnHotload()
	{
		Rebuild();
	}

}
