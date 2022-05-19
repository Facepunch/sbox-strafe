
using Sandbox;
using Sandbox.UI;
using Strafe.Leaderboards;
using Strafe.Utility;

namespace Strafe.UI;

[UseTemplate]
internal class ReplayListEntry : EasyListEntry<ReplayEntity>
{

	public Friend Friend { get; private set; }
	public string Time => Item.FinalFrame.Time.ToTime();

	public ReplayListEntry()
	{
		AddClass( "scoreboard-entry" );
	}

	protected override void OnSet()
	{
		base.OnSet();

		Friend = new Friend( Item.PlayerId );
	}

}
