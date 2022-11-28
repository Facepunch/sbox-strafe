
using Sandbox;
using Strafe.Api;
using Strafe.Api.Messages;
using Strafe.Map;
using Strafe.Players;
using Strafe.UI;
using Strafe.Utility;
using System;
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
	public async void OnStage( Strafe.Players.TimerEntity timer )
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

			Chatbox.AddChatEntry( To.Single( timer ), "Timer", msg, "timer" );

			return;
		}

		// storing these before the async operation so they don't get lost
		var stageFrame = timer.GrabFrame();
		var courseFrame = player.Stage( 0 ).GrabFrame();
		var stage = timer.Stage;

		if ( !CanSubmit() )
		{
			Chatbox.AddChatEntry( To.Everyone, "Timer", $"{client.Name} finished in {stageFrame.Time.ToTime()}", "timer" );
			return;
		}

		var runJson = System.Text.Json.JsonSerializer.Serialize( CompletionData.From( timer ) );
		var result = await Backend.Post<CompletionSubmitResult>( "completion/submit", runJson );

		PrintResult( client, stage, stageFrame, courseFrame, result );

		if ( result == null ) return;

		if(/* result.NewRank == 1 && */result.IsPersonalBest )
		{
			var replay = new Replay( client.PlayerId, timer.Frames.ToList() );

			var upload = new UploadReplay()
			{
				CompletionId = result.CompletionId,
				ReplayBase64 = Convert.ToBase64String( replay.ToBytes() )
			};

			var uploadResult = await Backend.Post<UploadReplayResult>( "completion/upload-replay", upload.Serialize() );

			if( result.NewRank == 1 )
			{
				StrafeGame.Current.SetWrReplay( replay );
			}
		}
	}

	private void PrintResult( Client client, int stage, TimerFrame stageFrame, TimerFrame courseFrame, CompletionSubmitResult result )
	{
		if ( result == null || !result.IsPersonalBest )
		{
			Chatbox.AddChatEntry( To.Single( client ), "Timer", $"Map finished in {stageFrame.Time.ToTime()}", "timer" );
			return;
		}

		var improvement = result.NewTime - result.OldTime;
		var completionMsg = $"{client.Name} finished the map in {stageFrame.Time.ToTime()}, improving by {improvement.ToTime()}";
		var diffFrame = StrafeGame.Current.CourseType == CourseTypes.Staged ? stageFrame : courseFrame;

		if ( CprEntity.TryGetDiff( stage, diffFrame, out var diff ) )
		{
			completionMsg += " | WR " + diff.Time.ToTime( true );
		}

		if ( result.NewRank == 1 )
		{
			Chatbox.AddChatEntry( To.Everyone, "Timer", "**WORLD RECORD**", "important" );
		}
		else if ( result.NewRank <= 5 )
		{
			Chatbox.AddChatEntry( To.Everyone, "Timer", "Top 5", "important" );
		}

		Chatbox.AddChatEntry( To.Everyone, "Timer", completionMsg, "timer" );
		Chatbox.AddChatEntry( To.Everyone, "Timer", $"New rank: {result.NewRank}, Old rank: {result.OldRank}", "timer" );
	}

	private bool CanSubmit()
	{
		var ent = Entity.All.FirstOrDefault( x => x is StrafeMapConfig ) as StrafeMapConfig;
		if ( !ent.IsValid() ) return true;
		if ( ent.State == MapStates.Released ) return true;

		return false;
	}

}
