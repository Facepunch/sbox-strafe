
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Strafe.UI;

/// <summary>
/// A button that will navigate to an href but also have .active class if href is active
/// </summary>
[Library( "navlink" )]
public class NavigatorButton : Button
{
	NavigatorPanel Navigator;
	public string HRef { get; set; }

	public override void OnParentChanged()
	{
		base.OnParentChanged();

		Navigator = Ancestors.OfType<NavigatorPanel>().FirstOrDefault();
	}

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name == "href" )
		{
			HRef = value;
		}
	}

	protected override void OnMouseDown( MousePanelEvent e )
	{
		if ( e.Button == "mouseleft" )
		{
			CreateEvent( "navigate", HRef );
			e.StopPropagation();
		}
	}

	public override void Tick()
	{
		base.Tick();

		var active = Navigator?.CurrentUrlMatches( HRef ) ?? false;
		SetClass( "active", active );
	}
}
