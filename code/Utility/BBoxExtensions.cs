
namespace Strafe.Utility;

internal static class BBoxExtensions
{

	public static bool Contains( this BBox box, Vector3 position )
	{
		return box.AddPoint( position ).Size == box.Size;
	}

}
