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
		var entry = ChildrenOfType<VoiceChatEntry>().FirstOrDefault( x => x.Friend.Id == steamId );
		if ( entry == null ) entry = new VoiceChatEntry( this, steamId );

		entry.Update( level );
	}
}
