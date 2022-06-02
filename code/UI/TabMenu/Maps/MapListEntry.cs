
using Sandbox.UI;

namespace Strafe.UI;

[UseTemplate]
internal class MapListEntry : EasyListEntry<MapListData>
{

	public Panel Thumbnail { get; set; }

	protected override async void OnSet()
	{
		base.OnSet();

		await Thumbnail.Style.SetBackgroundImageAsync( Item.Thumbnail );
	}

	public override void Tick()
	{
		base.Tick();

		var nominated = false;
		foreach( var kvp in StrafeGame.Current.Nominations )
		{
			if ( kvp.Value != Item.FullIdent ) continue;
			nominated = true;
			break;
		}

		SetClass( "nominated", nominated );
	}

	public void Nominate()
	{
		StrafeGame.Nominate( Item.FullIdent );
	}

}

public class MapListData
{
	public string FullIdent { get; set; }
	public string Name { get; set; }
	public string Thumbnail { get; set; }
	public string Author { get; set; }
	public string Description { get; set; }
}
