
using Sandbox.UI;

namespace Strafe.UI;

internal class EasyListEntry<T> : Panel
{

	public T Item { get; private set; }

	public void Set( T item )
	{
		Item = item;

		OnSet();
	}

	protected virtual void OnSet() { }

}
