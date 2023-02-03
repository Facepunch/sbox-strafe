
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
		if ( !Game.IsServer ) return;

		var player = timer.Owner as StrafePlayer;
		if ( !player.IsValid() ) return;

		var client = player.Client;
		if ( !client.IsValid() ) return;

		var snapshot = StrafeGame.Current.CourseType == CourseTypes.Staged
			? timer.GrabFrame()
			: player.Stage( 0 ).GrabFrame();

		var style = player.Style;

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

		PrintResult( client, stage, style, stageFrame, courseFrame, result );

		if ( result == null ) return;

		if(/* result.NewRank == 1 && */result.IsPersonalBest )
		{
			var replay = new Replay( client.SteamId, timer.Frames.ToList() );

			var upload = new UploadReplay()
			{
				CompletionId = result.CompletionId,
				ReplayBase64 = Convert.ToBase64String( replay.ToBytes() )
			};

			var uploadResult = await Backend.Post<UploadReplayResult>( "completion/upload-replay", upload.Serialize() );

			if( result.NewRank == 1 )
			{
				StrafeGame.Current.SetWrReplay( replay, style );
			}
		}
	}

	private void PrintResult( IClient client, int stage, TimerStyles style, TimerFrame stageFrame, TimerFrame courseFrame, CompletionSubmitResult result )
	{
		var timerName = "Timer";

		if( style != TimerStyles.Normal )
		{
			timerName += $" [{style}]";
		}

		if ( ( result?.Credits ?? 0 ) > 0 )
		{
			Chatbox.AddChatEntry( To.Single( client ), "Shop", $"You earned {result.Credits} \U0001fa99 for that run!", "store" );

			if ( client.Pawn is StrafePlayer pl )
			{
				pl.Credits += result.Credits;
			}
		}

		if ( result == null || !result.IsPersonalBest )
		{
			Chatbox.AddChatEntry( To.Single( client ), timerName, $"Map finished in {stageFrame.Time.ToTime()}", "timer" );
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
			Chatbox.AddChatEntry( To.Everyone, timerName, "**WORLD RECORD**", "important" );
		}
		else if ( result.NewRank <= 5 )
		{
			Chatbox.AddChatEntry( To.Everyone, timerName, "Top 5", "important" );
		}

		Chatbox.AddChatEntry( To.Everyone, timerName, completionMsg, "timer" );
		Chatbox.AddChatEntry( To.Everyone, timerName, $"New rank: {result.NewRank}, Old rank: {result.OldRank}", "timer" );
	}

	private bool CanSubmit()
	{
		var ent = Entity.All.FirstOrDefault( x => x is StrafeMapConfig ) as StrafeMapConfig;
		if ( !ent.IsValid() ) return true;
		if ( ent.State == MapStates.Released ) return true;

		return false;
	}

}
