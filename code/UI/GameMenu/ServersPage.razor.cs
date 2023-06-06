
namespace Strafe.UI;

public partial class ServersPage : Panel
{

	ServerList List;

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

}
