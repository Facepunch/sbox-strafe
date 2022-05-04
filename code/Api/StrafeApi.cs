
using Sandbox.Internal;
using System.Threading.Tasks;
using System.Text.Json;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Strafe.Api;

internal class StrafeApi
{

	public static string Endpoint => "https://localhost:7265/api";
	public static string WebSocketEndpoint => "wss://localhost:7265/api/ws";

	private static WebSocket WebSocket;
	private static int MessageIdAccumulator;
	private static List<GameMessage> Responses = new();

	public static async Task<T> Get<T>( string controller )
	{
		var http = new Http( new System.Uri( $"{Endpoint}/{controller}" ) );
		var result = await http.GetStringAsync();
		http.Dispose();

		return JsonSerializer.Deserialize<T>( result );
	}

	public static async Task<T> Post<T>( string controller, string jsonData )
	{
		if( !await EnsureWebSocket() )
		{
			Log.Error( "WebSocket failed to connect" );
			return default;
		}

		jsonData ??= string.Empty;

		var msg = new GameMessage()
		{
			Id = ++MessageIdAccumulator,
			Controller = controller,
			Message = jsonData,
		};

		await WebSocket.Send( JsonSerializer.Serialize( msg ) );
		var response = await WaitForResponse( msg.Id );

		if( response == null )
		{
			Log.Error( $"WebSocket response failed: {controller}" );
			return default;
		}

		return JsonSerializer.Deserialize<T>( response.Message );
	}

	private static async Task<GameMessage> WaitForResponse( int messageid, float timeout = 7f )
	{
		RealTimeUntil tu = timeout;
		while( tu > 0 )
		{
			var response = Responses.FirstOrDefault( x => x.Id == messageid );
			if ( response != null ) return response;

			await Task.Delay( 100 );
		}
		return null;
	}

	private static async Task<bool> EnsureWebSocket()
	{
		if ( WebSocket?.IsConnected ?? false ) return true;

		WebSocket?.Dispose();
		WebSocket = new();
		WebSocket.OnMessageReceived += WebSocket_OnMessageReceived;
		await WebSocket.Connect( WebSocketEndpoint );

		return WebSocket.IsConnected;
	}

	[Event.Tick]
	public static void OnTick()
	{
		for ( int i = Responses.Count - 1; i >= 0; i-- )
		{
			if ( (Responses[i]?.TimeSinceReceived ?? 0) > 30 )
			{
				Responses.RemoveAt( i );
			}
		}
	}

	private static void WebSocket_OnMessageReceived( string message )
	{
		try
		{
			var msg = JsonSerializer.Deserialize<GameMessage>( message );
			msg.TimeSinceReceived = 0;
			Responses.Add( msg );
		}
		catch( System.Exception e )
		{
			Log.Error( e.Message );
		}
	}

	public class GameMessage
	{
		public int Id { get; set; }
		public string Controller { get; set; }
		public string Message { get; set; }

		public TimeSince TimeSinceReceived;
	}

}
