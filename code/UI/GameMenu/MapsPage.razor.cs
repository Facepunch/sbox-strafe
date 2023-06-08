
namespace Strafe.UI;

public partial class MapsPage : Panel
{

	List<Package> Maps = new();
	DropDown MapTypeDropDown;

	MapTypes MapTypeFilter { get; set; } = MapTypes.None;
	MapDifficulties MapDifficultyFilter { get; set; } = MapDifficulties.None;

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime )
			return;

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

	bool Filter( Package pkg )
	{
		var mapinfo = MapInfo.Get( pkg.FullIdent );

		if ( MapTypeFilter != MapTypes.None && mapinfo.Type != MapTypeFilter )
			return false;

		if ( MapDifficultyFilter != MapDifficulties.None && mapinfo.Difficulty != (int)MapDifficultyFilter )
			return false;

		return true;
	}

	void FilterRefresh( string _ )
	{
		StateHasChanged();
	}

}
