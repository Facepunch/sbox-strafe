
namespace Strafe.Api.Messages;

public class PlayerData
{

	public long SteamId { get; set; }
	public string Name { get; set; }
	public int Credits { get; set; }
	public int Points { get; set; }
	public DateTimeOffset FirstSeen { get; set; }
	public DateTimeOffset LastSeen { get; set; }

}
