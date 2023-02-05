
namespace Strafe.Leaderboards;

internal partial class ReplayEntity : AnimatedEntity
{

	private int NumberOfLoops;
	private int CurrentLoop;
	private int CurrentFrame;
	private Replay Replay;

	[Net]
	public long PlayerId { get; set; }
	[Net]
	public TimerFrame FinalFrame { get; set; }
	[Net]
	public TimerFrame Frame { get; set; }
	[Net]
	public TimerStyles Style { get; set; }
	[Net]
	public Rotation EyeRotation { get; set; }

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
		Frame = Replay.Frames[CurrentFrame];

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

	[Event.Tick.Client]
	public void OnTickClient()
	{
		if ( Game.LocalPawn is not StrafePlayer pl ) return;
		var a = pl.SpectateTarget == this ? 0 : .5f;
		RenderColor = RenderColor.WithAlpha( a );
	}

	private void ApplyFrame( TimerFrame frame )
	{
		Position = frame.Position;
		Rotation = Rotation.From( frame.Angles.WithPitch( 0 ) );
		EyeRotation = Rotation.From( frame.Angles );
		//EyeRotation = Rotation.From( frame.Angles );
		//EyeLocalPosition = Vector3.Up * 64f;
		Velocity = frame.Velocity;
	}

	public static ReplayEntity Play( Replay replay, int loops, TimerStyles style )
	{
		//Host.AssertServer();

		return new ReplayEntity()
		{
			Replay = replay,
			NumberOfLoops = loops,
			Style = style
		};
	}

}
