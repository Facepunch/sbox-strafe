
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

	public void Nominate()
	{
		Log.Error( "NOMINATE: " + Item.Ident );
	}

}

public class MapListData
{
	public string Ident { get; set; }
	public string Name { get; set; }
	public string Thumbnail { get; set; }
	public string Author { get; set; }
	public string Description { get; set; }
}
