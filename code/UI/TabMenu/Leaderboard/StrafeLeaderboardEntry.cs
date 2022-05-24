
using Strafe.Api.Messages;
using Sandbox.UI;
using Strafe.Utility;

namespace Strafe.UI;

[UseTemplate]
internal class StrafeLeaderboardEntry : EasyListEntry<PersonalBestEntry>
{

	public string FormattedTime { get; set; }

	protected override void OnSet()
	{
		base.OnSet();

		FormattedTime = Item.Time.ToTime();
	}

}
