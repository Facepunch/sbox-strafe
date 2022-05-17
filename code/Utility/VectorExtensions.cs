
namespace Strafe.Utility;

internal static class VectorExtensions
{

	public static string ToHuman( this Vector3 vec, bool horizontal = false )
	{
		var spd = (horizontal ? vec.WithZ( 0 ) : vec).Length;
		return $"{(int)spd} u/s";
	}

}
