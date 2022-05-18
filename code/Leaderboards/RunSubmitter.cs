
using Sandbox;
using Strafe.Api;
using Strafe.Api.Messages;
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

	[Events.Timer.OnStageComplete]
	public async void OnStage( TimerEntity timer )
	{
		if ( !IsServer ) return;

		var player = timer.Owner as StrafePlayer;
		if ( !player.IsValid() ) return;

		var client = player.Client;
		if ( !client.IsValid() ) return;

		var snapshot = StrafeGame.Current.CourseType == CourseTypes.Staged
			? timer.GrabFrame()
			: player.Stage( 0 ).GrabFrame();

		CprEntity.TryGetDiff( timer.Stage, snapshot, out var diff );

		if ( timer.Stage > 0 )
		{
			var term = StrafeGame.Current.CourseType == CourseTypes.Linear ? "cp" : "stage ";
			var msg = $"{term}{timer.Stage} finished in {snapshot.Time.ToTime()}";

			if ( diff != default )
			{
				msg += " | WR " + diff.Time.ToTime( true );
			}

			Chat.AddChatEntry( To.Single( timer ), "Timer", msg );

			return;
		}

		var replay = new Replay( timer.Frames.ToList() );
		ReplayEntity.Play( replay, 5 );

		//var replayJson = System.Text.Json.JsonSerializer.Serialize( replay );
		//we can send replay data over somehow as well

		var runJson = System.Text.Json.JsonSerializer.Serialize( CompletionData.From( timer ) );
		var result = await Backend.Post<CompletionSubmitResult>( "completion/submit", runJson );

		if( result != null )
		{
			PrintResult( client, timer, result );
		}
	}

	private void PrintResult( Client client, TimerEntity timer, CompletionSubmitResult result )
	{
		if ( result.OldTime != 0 && result.OldTime < result.NewTime ) 
			return;
		if ( client.Pawn is not StrafePlayer player ) 
			return;

		var improvement = result.NewTime - result.OldTime;
		var completionMsg = $"{client.Name} finished the map in {timer.Timer.ToTime()}, improving by {improvement.ToTime()}";
		var snapshot = StrafeGame.Current.CourseType == CourseTypes.Staged
			? timer.GrabFrame()
			: player.Stage( 0 ).GrabFrame();

		CprEntity.TryGetDiff( timer.Stage, snapshot, out var diff );

		if ( diff != default )
		{
			completionMsg += " | WR " + diff.Time.ToTime( true );
		}

		if ( result.NewRank == 1 )
		{
			Chat.AddChatEntry( To.Everyone, "Timer", "**WORLD RECORD**", "important" );
		}
		else if ( result.NewRank <= 5 )
		{
			Chat.AddChatEntry( To.Everyone, "Timer", "Top 5", "important" );
		}

		Chat.AddChatEntry( To.Everyone, "Timer", completionMsg );
		Chat.AddChatEntry( To.Everyone, "Timer", $"New rank: {result.NewRank}, Old rank: {result.OldRank}" );
	}

}
