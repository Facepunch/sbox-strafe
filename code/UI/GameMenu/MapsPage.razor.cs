
namespace Strafe.UI;

public partial class MapsPage : Panel
{

	enum SortMode 
	{
		None,
		HoursPlayed,
		MostUniquePlayers,
		Newest,
		Oldest
	}

	List<Package> Maps = new();
	DropDown MapTypeDropDown;

	MapTypes MapTypeFilter { get; set; } = MapTypes.None;
	MapDifficulties MapDifficultyFilter { get; set; } = MapDifficulties.None;
	SortMode SortModeFilter { get; set; } = SortMode.Newest;

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

	IEnumerable<Package> Sort( List<Package> stuff )
	{
		if ( SortModeFilter == SortMode.None )
			return stuff;

		if ( SortModeFilter == SortMode.HoursPlayed )
			return stuff.OrderByDescending( x => x.Usage.Total.Seconds );

		if ( SortModeFilter == SortMode.MostUniquePlayers )
			return stuff.OrderByDescending( x => x.Usage.Total.Users );

		if ( SortModeFilter == SortMode.Newest )
			return stuff.OrderByDescending( x => x.Created );

		if ( SortModeFilter == SortMode.Oldest )
			return stuff.OrderBy( x => x.Created );

		return stuff;
	}

	bool Filter( Package pkg )
	{
		var mapinfo = MapInfo.Get( pkg.FullIdent );

		if ( MapTypeFilter != MapTypes.None && mapinfo.Type != MapTypeFilter )
			return false;

		if ( MapDifficultyFilter != MapDifficulties.None && mapinfo.Difficulty != MapDifficultyFilter )
			return false;

		return true;
	}

	void FilterRefresh( string _ )
	{
		StateHasChanged();
	}

}
