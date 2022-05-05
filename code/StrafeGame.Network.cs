
using Sandbox;
using Strafe.Api;
using Strafe.Utility;

namespace Strafe;

internal partial class StrafeGame
{

	private async void NetworkLogin( Client client )
	{
		Host.AssertServer();

		var msg = new LoginMessage()
		{
			Host = Global.ServerSteamId.ToString(),
			Name = client.Name,
			PlayerId = client.PlayerId
		};

		await StrafeApi.Post<bool>( "player/login", msg.Serialize() );
	}

}
