
using Sandbox;

namespace Strafe.Utility;

internal static class EntityExtensions
{

	public static void SetRenderAlphaRecursive( this Entity e, float a )
	{
		if ( !e.IsValid() ) return;

		if ( e is ModelEntity m )
			m.RenderColor = m.RenderColor.WithAlpha( a );

		foreach ( var child in e.Children )
		{
			child.SetRenderAlphaRecursive( a );
		}
	}

}
