
using System.Linq;

namespace Strafe.UI;

public partial class MapsPage : Panel
{

	List<Package> Maps = new();

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( firstTime )
			Refresh();
	}

	async void Refresh()
	{
		var mapidents = await StrafeGame.GetAvailableMaps();

		foreach ( var ident in mapidents.Distinct() )
		{
			var pkg = await Package.Fetch( ident, true );
			if ( pkg == null ) continue;
			Maps.Add( pkg );
		}
	}

}
