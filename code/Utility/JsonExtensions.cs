
using System.Text.Json;

namespace Strafe.Utility;

internal static class JsonExtensions
{

	public static string Serialize( this object obj )
	{
		return JsonSerializer.Serialize( obj );
	}

	public static T Deserialize<T>( this string str )
	{
		return JsonSerializer.Deserialize<T>( str );
	}

}
