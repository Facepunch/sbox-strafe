
using Sandbox;
using Strafe.Map;
using Strafe.Replays;
using Strafe.UI;
using Strafe.Utility;
using System.Collections.Generic;
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
	public int Jumps { get; set; }
	[Net, Predicted]
	public int Strafes { get; set; }


	private List<TimerFrame> frames = new( 360000 );
	public IReadOnlyList<TimerFrame> Frames => frames;

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
		frames.Clear();
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

	public void Complete()
	{
		if ( State != States.Live )
			return;

		State = States.Complete;

		Events.Timer.OnStage.Run( this );
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

		frames.Add( GrabFrame() );
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

	private TimerFrame GrabFrame()
	{
		return new TimerFrame()
		{
			Velocity = Owner.Velocity,
			Position = Owner.Position,
			Angles = Owner.Rotation.Angles(),
			Time = Timer,
			Jumps = Jumps,
			Strafes = Strafes
		};
	}

}
