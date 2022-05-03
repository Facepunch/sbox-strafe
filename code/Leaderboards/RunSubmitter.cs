
using Sandbox;
using Strafe.Players;
using Strafe.Replays;
using Strafe.UI;
using Strafe.Utility;

namespace Strafe.Leaderboards;

internal class RunSubmitter : Entity
{

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	[Events.Timer.OnStage]
	public async void OnStage( TimerEntity timer )
	{
		if ( !IsServer ) return;

		var player = timer.Owner as StrafePlayer;
		if ( !player.IsValid() ) return;

		var client = player.Client;
		if ( !client.IsValid() ) return;

		// todo: we want linear checkpoints to be based off of overall time
		// and stats rather than offset like stages
		var term = StrafeGame.Current.CourseType == CourseTypes.Linear
			? "cp"
			: "stage";

		var thing = timer.Stage == 0
			? "the course"
			: $"{term} {timer.Stage}";

		Chat.AddChatEntry( To.Everyone, "Server", $"{client.Name} finished {thing} in {timer.Timer.HumanReadable()}s" );

		if ( timer.Stage != 0 ) return;

		var replay = Replay.Create( timer.Frames, client.PlayerId );
		ReplayEntity.Play( replay, 5 );

		var result = await GameServices.SubmitScore( client.PlayerId, timer.Timer );
		PrintResult( client.PlayerId, result );
	}

	public static void PrintResult( long playerid, SubmitScoreResult result )
	{
		if ( result.ScoreDelta == 0 ) return;

		if ( result.NewRank == 1 )
		{
			Chat.AddChatEntry( To.Everyone, "Server", "WORLD RECORD!!", "bold purple" );
			Chat.AddChatEntry( To.Everyone, "Server", "WORLD RECORD!!", "bold purple" );
		}

		Chat.AddChatEntry( To.Everyone, "Server", $"Old rank: {result.OldRank} - New rank: {result.NewRank} - Improvement: {result.ScoreDelta.HumanReadable()}s", "bold" );
	}

}
