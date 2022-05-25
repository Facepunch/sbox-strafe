
using System;

namespace Strafe.Utility;

internal static class VectorExtensions
{

	public static string ToHuman( this Vector3 vec, bool horizontal = false )
	{
		var spd = (horizontal ? vec.WithZ( 0 ) : vec).Length;
		return $"{(int)spd} u/s";
	}

	public static Vector3 Clip( this Vector3 input, Vector3 normal, float overbounce = 1.0f )
	{
		var backoff = Vector3.Dot( input, normal ) * overbounce;
		var o = input - (normal * backoff);

		var adjust = Vector3.Dot( o, normal );
		if ( adjust >= 1.0f ) return o;

		adjust = MathF.Min( adjust, -1.0f );
		o -= normal * adjust;

		return o;
	}

}
