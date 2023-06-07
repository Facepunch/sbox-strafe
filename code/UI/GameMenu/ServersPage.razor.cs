
namespace Strafe.UI;

public partial class ServersPage : Panel
{

	ServerList List;
	Button ConnectButton;

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

	public override void Tick()
	{
		base.Tick();

		ConnectButton.SetClass( "disabled", selected == null );

		if ( selected?.IsDeleting ?? false )
			selected = null;
	}

	bool refreshing;
	async void Refresh()
	{
		if ( refreshing ) return;

		refreshing = true;
		List?.Dispose();
		List = new();
		List.AddFilter( "gametagsand", "game:facepunch.strafe" );
		List.Query();

		while ( List.IsQuerying )
			await Task.Delay( 100 );

		StateHasChanged();

		refreshing = false;
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

	ServerEntry selected;
	public void SetSelected( ServerEntry e )
	{
		selected?.RemoveClass( "active" );
		selected = e;
		selected?.AddClass( "active" );
	}

	public void Join( ServerList.Entry server )
	{
		Game.Menu.ConnectToServer( server.SteamId );
	}

	void JoinSelected()
	{
		if ( selected == null ) return;

		Join( selected.Entry );
	}

}
