
using Sandbox;
using Strafe.Map;
using Strafe.UI;
using Strafe.Utility;
using System.Linq;

namespace Strafe.Players;

internal partial class TimerEntity : Entity
{

	public enum States
	{
		Stopped,
		Start,
		Live,
		Complete
	}

	[Net, Predicted]
	public States State { get; set; }
	[Net, Predicted]
	public int Stage { get; set; }
	[Net, Predicted]
	public float Timer { get; set; }
	[Net, Predicted]
	public bool Current { get; set; }
	[Net, Predicted]
	public int Checkpoint { get; set; }
	[Net, Predicted]
	public int Jumps { get; set; }
	[Net, Predicted]
	public int Strafes { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Owner;
	}

	private void Reset()
	{
		Timer = 0f;
		Jumps = 0;
		Strafes = 0;
		Checkpoint = 0;
	}

	public void Start()
	{
		Reset();
		State = States.Live;
	}

	public void Stop()
	{
		Reset();
		State = States.Stopped;
	}

	public async void Complete()
	{
		if ( State != States.Live )
			return;

		State = States.Complete;

		if ( IsServer )
		{
			var thing = Stage == 0
				? "the course"
				: $"stage {Stage}";

			Chat.AddChatEntry( To.Everyone, "Server", $"{Owner.Client.Name} finished {thing} in {Timer.HumanReadable()}s" );

			if ( Stage != 0 ) return;

			var result = await GameServices.SubmitScore( Owner.Client.PlayerId, Timer );

			PrintResult( Owner.Client.PlayerId, result );
		}
	}

	public void SetCheckpoint( int checkpoint )
	{
		if ( checkpoint <= Checkpoint ) 
			return;

		if ( State != States.Live )
			return;

		Checkpoint = checkpoint;

		if ( IsServer )
		{
			Chat.AddChatEntry( To.Single( Owner.Client ), "Server", $"CP {checkpoint} in {Timer.HumanReadable()}s" );
		}
	}

	public void SetCurrent()
	{
		Owner.Children.OfType<TimerEntity>()
			.ToList()
			.ForEach( x => x.Current = false );
		
		State = States.Start;
		Current = true;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( Owner is not StrafePlayer pl )
		{
			Log.Error( "This shouldn't happen" );
			return;
		}

		if( pl.GetActiveController() is not StrafeController )
			Stop();

		if ( State != States.Live ) 
			return;

		Timer += Time.Delta;
	}

	public void TeleportTo()
	{
		if ( Owner is not StrafePlayer pl ) 
			return;

		var targetStage = Stage == 0 ? 1 : Stage;

		var start = All.First( x => x is StageStart s && s.Stage == targetStage );
		var pos = start.WorldSpaceBounds.Center;
		var height = start.WorldSpaceBounds.Size.z;
		var tr = Trace.Ray( pos, pos + Vector3.Down * height * .55f )
			.HitLayer( CollisionLayer.All, false )
			.HitLayer( CollisionLayer.Solid, true )
			.HitLayer( CollisionLayer.GRATE, true )
			.HitLayer( CollisionLayer.PLAYER_CLIP, true )
			.HitLayer( CollisionLayer.WINDOW, true )
			.HitLayer( CollisionLayer.NPC, true )
			.WithoutTags( "player" )
			.Run();

		if ( tr.Hit )
			pos = tr.EndPosition + Vector3.Up;

		pl.Position = pos;
		pl.Rotation = start.Rotation;
	}

	public static void PrintResult( long playerid, SubmitScoreResult result )
	{
		if ( result.ScoreDelta == 0 ) return;

		if( result.NewRank == 1 )
		{
			Chat.AddChatEntry( To.Everyone, "Server", "WORLD RECORD!!", "bold purple" );
			Chat.AddChatEntry( To.Everyone, "Server", "WORLD RECORD!!", "bold purple" );
		}

		Chat.AddChatEntry( To.Everyone, "Server", $"Old rank: {result.OldRank} - New rank: {result.NewRank} - Improvement: {result.ScoreDelta.HumanReadable()}s", "bold" );
	}

}
