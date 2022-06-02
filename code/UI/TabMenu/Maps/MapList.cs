
using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Strafe.UI;

internal class MapList : EasyList<MapListEntry, MapListData>
{

	protected override async Task<List<MapListData>> FetchItemsAsync()
	{
		var mapidents = await StrafeGame.GetAvailableMaps();
		var result = new List<MapListData>();

		foreach( var ident in mapidents )
		{
			var pkg = await Package.Fetch( ident, true );
			result.Add( new()
			{
				Ident = pkg.Ident,
				Name = pkg.Title,
				Thumbnail = pkg.Thumb,
				Description = pkg.Description
			} );
		}

		return result;
	}

}
