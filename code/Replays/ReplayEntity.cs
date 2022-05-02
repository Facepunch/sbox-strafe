
using Sandbox;
using Strafe.Players;

namespace Strafe.Replays;

internal class ReplayEntity : AnimEntity
{

	private int NumberOfLoops;
	private int CurrentLoop;
	private int CurrentFrame;
	private Replay Replay;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		EnableAllCollisions = false;
	}

	[Event.Tick.Server]
	public void OnTick()
	{
		if ( Replay == null ) return;
		if ( Replay.Frames == null || Replay.Frames.Count == 0 ) return;

		ApplyFrame( Replay.Frames[CurrentFrame] );

		CurrentFrame++;

		if ( CurrentFrame >= Replay.Frames.Count )
		{
			CurrentFrame = 0;
			CurrentLoop++;
			ResetInterpolation();

			if( NumberOfLoops > 0 && CurrentLoop >= NumberOfLoops )
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
