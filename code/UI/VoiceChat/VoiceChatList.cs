using System.Linq;

public class VoiceChatList : Panel{
	public static VoiceChatList Current { get; internal set; }

	public VoiceChatList()
	{
		Current = this;
		StyleSheet.Load( "/UI/VoiceChat/VoiceChatList.scss" );
	}

	public void OnVoicePlayed( long steamId, float level )
	{
		var entry = ChildrenOfType<StrafeVoiceChatEntry>().FirstOrDefault( x => x.Friend.Id == steamId );
		if ( entry == null ) entry = new StrafeVoiceChatEntry( this, steamId );

		entry.Update( level );
	}
}
