
namespace Strafe.Api;

internal class LoginMessage
{

	public long PlayerId { get; set; }
	public string Name { get; set; }
	public long ServerSteamId { get; set; }
	public string MapIdent { get; set; }

}
