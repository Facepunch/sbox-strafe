using Sandbox.UI.Construct;

public class VoiceChatEntry : Panel
{
	public Friend Friend;
	
	readonly Label Name;
	readonly Image Avatar;

	private float VoiceLevel = 0.0f;
	private float TargetVoiceLevel = 0;

	RealTimeSince timeSincePlayed;

	public VoiceChatEntry( Panel parent, long steamId )
	{
		Parent = parent;
		
		Friend = new Friend( steamId );

		Name = Add.Label( Friend.Name, "name" );

		Avatar = Add.Image( "", "avatar" );
		Avatar.SetTexture( $"avatar:{steamId}" );
	}

	public void Update( float level )
	{
		timeSincePlayed = 0;
		TargetVoiceLevel = level;
	}

	public override void Tick()
	{
		base.Tick();

		if ( IsDeleting )
			return;

		var SpeakTimeout = 2.0f;
		var timeoutInv = 1 - (timeSincePlayed / SpeakTimeout);
		timeoutInv = MathF.Min( timeoutInv * 2.0f, 1.0f );

		if ( timeoutInv <= 0 )
		{
			Delete();
			return;
		}

		Style.Dirty();
	}
}
