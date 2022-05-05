
using Sandbox;
using Strafe.Api;
using Strafe.Players;
using Strafe.UI;
using Strafe.Utility;
using System.Linq;

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

		Chat.AddChatEntry( To.Everyone, "Server", $"{client.Name} finished {thing} in {timer.Timer.ToTime()}s" );

		if ( timer.Stage != 0 ) return;

		var replay = new Replay( timer.Frames.ToList() );
		ReplayEntity.Play( replay, 5 );

		//var replayJson = System.Text.Json.JsonSerializer.Serialize( replay );
		//we can send replay data over somehow as well

		var runJson = System.Text.Json.JsonSerializer.Serialize( StageSubmission.From( timer ) );
		var result = await StrafeApi.Post<string>( "stage/submit", runJson );

		Chat.AddChatEntry( To.Everyone, "Response", result );
	}

	public static void PrintResult( long playerid, SubmitScoreResult result )
	{
		if ( result.ScoreDelta == 0 ) return;

		if ( result.NewRank == 1 )
		{
			Chat.AddChatEntry( To.Everyone, "Server", "WORLD RECORD!!", "bold purple" );
			Chat.AddChatEntry( To.Everyone, "Server", "WORLD RECORD!!", "bold purple" );
		}

		Chat.AddChatEntry( To.Everyone, "Server", $"Old rank: {result.OldRank} - New rank: {result.NewRank} - Improvement: {result.ScoreDelta.ToTime()}s", "bold" );
	}

}
