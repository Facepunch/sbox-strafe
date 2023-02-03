
namespace Strafe.Api.Messages;

internal class ClientLogin
{

	public long PlayerId { get; set; }
	public string Name { get; set; }
	public long ServerSteamId { get; set; }
	public string MapIdent { get; set; }

}

public class ClientLoginResult
{

	public long PlayerId { get; set; }
	public int TotalCredits { get; set; }
	public int CreditsAwarded { get; set; }

}
