using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Strafe.UI;

internal abstract class EasyList<T, T2> : Panel
	where T : EasyListEntry<T2>
{

	public Panel Canvas { get; set; }

	private int activehash;

	protected virtual List<T2> FetchItems() => null;
	protected virtual int GetItemHash() => 0;

	protected void Rebuild()
	{
		var items = FetchItems();
		if ( items == null ) return;

		var parent = Canvas ?? this;
		parent?.DeleteChildren( true );

		foreach ( var item in items )
		{
			var child = Library.Create<T>() as EasyListEntry<T2>;
			child.Set( item );
			child.Parent = parent;
		}
	}

	protected override void PostTemplateApplied() => Rebuild();
	public override void OnHotloaded() => Rebuild();

	public override void Tick()
	{
		base.Tick();

		var hash = GetItemHash();

		if ( activehash == hash ) return;
		activehash = hash;

		Rebuild();
	}

}
