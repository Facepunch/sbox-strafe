
using Sandbox.Internal;
using System.Threading.Tasks;
using System.Text.Json;
using Sandbox;

namespace Strafe.Api;

internal class StrafeApi
{

	public static string Endpoint => "https://localhost:7265/api";
	public static string WebSocketEndpoint => "wss://localhost:7265/api/ws";

	private static WebSocket WebSocket;
	private static int MessageIdAccumulator;

	public static async Task<T> Fetch<T>( string controller )
	{
		var http = new Http( new System.Uri( $"{Endpoint}/{controller}" ) );
		var result = await http.GetStringAsync();
		http.Dispose();

		return JsonSerializer.Deserialize<T>( result );
	}

	public static async Task<T> Send<T>( string controller, string jsonData )
	{
		if( !await EnsureWebSocket() )
		{
			Log.Error( "WebSocket failed to connect" );
			return default;
		}

		var msg = new TwoWayMessage()
		{
			MessageId = ++MessageIdAccumulator,
			Controller = controller,
			Message = jsonData,
		};

		await WebSocket.Send( JsonSerializer.Serialize( msg ) );

		return default;
	}

	private static async Task<bool> EnsureWebSocket()
	{
		if ( WebSocket?.IsConnected ?? false ) return true;

		WebSocket?.Dispose();
		WebSocket = new();
		WebSocket.OnDataReceived += WebSocket_OnDataReceived;
		WebSocket.OnMessageReceived += WebSocket_OnMessageReceived;
		await WebSocket.Connect( WebSocketEndpoint );

		return WebSocket.IsConnected;
	}

	private static void WebSocket_OnMessageReceived( string message )
	{
		Log.Error( "Recieved message: " + message );
	}

	private static void WebSocket_OnDataReceived( System.Span<byte> data )
	{
		Log.Error( "Received data: " + data.Length );
	}

	public class TwoWayMessage
	{
		public int MessageId { get; set; }
		public string Controller { get; set; }
		public string Message { get; set; }
		public string Response { get; set; }
	}

}
