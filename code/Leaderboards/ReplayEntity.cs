
using Sandbox;
using Strafe.Players;

namespace Strafe.Leaderboards;

internal partial class ReplayEntity : AnimEntity
{

	private int NumberOfLoops;
	private int CurrentLoop;
	private int CurrentFrame;
	private Replay Replay;

	[Net]
	public long PlayerId { get; set; }
	[Net]
	public TimerFrame FinalFrame { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		EnableAllCollisions = false;

		Transmit = TransmitType.Always;
	}

	[Event.Tick.Server]
	public void OnTick()
	{
		if ( Replay == null ) return;
		if ( Replay.Frames == null || Replay.Frames.Count == 0 ) return;

		PlayerId = Replay.PlayerId;
		FinalFrame = Replay.Frames[^1];

		ApplyFrame( Replay.Frames[CurrentFrame] );

		CurrentFrame++;

		if ( CurrentFrame >= Replay.Frames.Count )
		{
			CurrentFrame = 0;
			CurrentLoop++;
			ResetInterpolation();

			if ( NumberOfLoops > 0 && CurrentLoop >= NumberOfLoops )
			{
				Delete();
			}
		}
	}

	private void ApplyFrame( TimerFrame frame )
	{
		Position = frame.Position;
		Rotation = Rotation.From( frame.Angles );
		Velocity = frame.Velocity;
	}

	public static void Play( Replay replay, int loops )
	{
		Host.AssertServer();

		new ReplayEntity()
		{
			Replay = replay,
			NumberOfLoops = loops
		};
	}

}
