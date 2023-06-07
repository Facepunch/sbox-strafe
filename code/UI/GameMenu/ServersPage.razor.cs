
namespace Strafe.UI;

public partial class ServersPage : Panel
{

	ServerList List;

	bool showEmpty = true;
	bool ShowEmpty
	{
		get => showEmpty;
		set
		{
			showEmpty = value;
			StateHasChanged();
		}
	}

	bool whitelistedOnly = false;
	bool WhitelistedOnly
	{
		get => whitelistedOnly;
		set
		{
			whitelistedOnly = value;
			StateHasChanged();
		}
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime )
			return;

		Refresh();
	}

	async void Refresh()
	{
		List?.Dispose();
		List = new();
		List.AddFilter( "gametagsand", "game:facepunch.strafe" );
		List.Query();

		while ( List.IsQuerying )
			await Task.Delay( 100 );

		StateHasChanged();
	}

	bool Filter( ServerList.Entry e )
	{
		if ( !showEmpty && e.Players == 0 )
			return false;

		if ( whitelistedOnly && !IsWhitelisted( e.SteamId ) )
			return false;

		return true;
	}

	bool IsWhitelisted( ulong steamid )
	{
		return false;
	}

}
