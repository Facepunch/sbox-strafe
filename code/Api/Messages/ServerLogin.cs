
namespace Strafe.Api.Messages;

internal class ServerLogin
{
	public ulong SteamId { get; set; }
	public string ServerName { get; set; }
	public string MapIdent { get; set; }
	public string MapTitle { get; set; }
	public CourseTypes CourseType { get; set; }
}
