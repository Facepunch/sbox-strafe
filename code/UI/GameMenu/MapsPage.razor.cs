
using Sandbox.Surf;

namespace Strafe.UI;

public record struct MapReference( Package Package, SurfMapAsset Asset )
{
	public static implicit operator MapReference( Package package )
	{
		return new MapReference( package, null );
	}

	public static implicit operator MapReference( SurfMapAsset asset )
	{
		return new MapReference( null, asset );
	}

	public string FullIdent => Package?.FullIdent ?? $"asset#{Asset.ResourceId}";
	public string Title => Package?.Title ?? Asset?.Title;
	public string Description => Package?.Description ?? Asset?.Description;
	public string Thumb => Package?.Thumb ?? "";
	public string Author => Package?.Org.Title ?? Asset?.Author;

	public DateTimeOffset Created => Package?.Created ?? Asset?.Created ?? DateTimeOffset.UnixEpoch;
	public Package.PackageUsageStats Usage => Package?.Usage ?? default;

	public bool IsNull => Package == null && Asset == null;
}

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

	List<MapReference> Maps = new();
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

		foreach ( var surfMap in ResourceLibrary.GetAll<SurfMapAsset>() )
		{
			Maps.Add( surfMap );
		}
	}

	IEnumerable<MapReference> Sort( List<MapReference> stuff )
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

	bool Filter( MapReference mapRef )
	{
		var mapinfo = MapInfo.Get( mapRef );

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
