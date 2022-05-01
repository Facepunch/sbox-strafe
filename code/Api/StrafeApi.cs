
using Sandbox.Internal;
using System.Threading.Tasks;
using System.Text.Json;

namespace Strafe.Api;

internal class StrafeApi
{

	public static string Endpoint => "https://localhost:7265/api";

	public static async Task<T> Fetch<T>( string controller )
	{
		var http = new Http( new System.Uri( $"{Endpoint}/{controller}" ) );
		var result = await http.GetStringAsync();
		http.Dispose();

		return JsonSerializer.Deserialize<T>( result );
	}

}
