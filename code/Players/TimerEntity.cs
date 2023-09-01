
using Sandbox;
using Strafe.Map;
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

	[Net]
	public States State { get; set; }
	[Net]
	public int Stage { get; set; }
	[Net]
	public float Timer { get; set; }
	[Net]
	public bool Current { get; set; }
	[Net]
	public int Jumps { get; set; }
	[Net]
	public int Strafes { get; set; }
	[Net]
	public bool EnforceGroundState { get; set; }
	[Net]
	public int TotalSync { get; set; }
	[Net]
	public int GoodSync { get; set; }

	public TimerFrame Snapshot { get; private set; }


	private bool Linear => StrafeGame.Current.CourseType == CourseTypes.Linear;
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
		TotalSync = 0;
		GoodSync = 0;
		frames.Clear();

		Events.Timer.OnReset.Run( this );
	}

	public void Start()
	{
		if ( EnforceGroundState )
		{
			if ( Game.IsServer )
			{
				var msg = $"Timer didn't start, stand in the start zone after using noclip!";
				Chatbox.AddChatEntry( To.Single( Owner ), "Timer", msg, "timer" );
			}
			return;
		}

		Reset();
		State = States.Live;

		Events.Timer.OnStageStart.Run( this );

		if ( Prediction.FirstTime && Game.IsClient && Stage == 0 )
		{
			var msg = $"Timer started | {Owner.Velocity.ToHuman()}";
			Chatbox.AddChatEntry( "Timer", msg, "timer" );
		}
	}

	public void Stop()
	{
		Reset();
		State = States.Stopped;

		Events.Timer.OnStageStop.Run( this );
	}

	public void Complete()
	{
		if ( State != States.Live )
			return;

		State = States.Complete;
		Snapshot = Linear ? (Owner as StrafePlayer).Stage( 0 ).GrabFrame() : GrabFrame();

		Events.Timer.OnStageComplete.Run( this );
	}

	public void SetCurrent()
	{
		Owner.Children.OfType<TimerEntity>()
			.ToList()
			.ForEach( x => x.Current = false );

		State = States.Start;
		Current = true;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( Owner is not StrafePlayer pl )
		{
			Log.Error( "This shouldn't happen" );
			return;
		}

		if ( pl.Controller is not StrafeController ctrl || ctrl.Noclip )
		{
			Stop();
			EnforceGroundState = true;
			return;
		}

		if ( State != States.Live )
			return;

		Timer += Time.Delta;

		frames.Add( GrabFrame() );
	}

	public void TeleportTo()
	{
		if ( Owner is not StrafePlayer pl )
			return;

		pl.Transform = TeleportTransform() ?? pl.Transform;
	}

	public Transform? TeleportTransform()
	{
		var isLinear = StrafeGame.Current.CourseType == CourseTypes.Linear;
		StrafeTrigger targetTrigger = null;

		if ( isLinear && Stage > 0 )
		{
			targetTrigger = All.FirstOrDefault( x => x is LinearCheckpoint cp && cp.Checkpoint == Stage ) as StrafeTrigger;
		}
		else
		{
			var targetStage = Stage == 0 ? 1 : Stage;
			targetTrigger = All.First( x => x is StageStart s && s.Stage == targetStage ) as StrafeTrigger;
		}

		return targetTrigger?.SpawnTransform();
	}

	public TimerFrame GrabFrame()
	{
		var sync = TotalSync > 0
			? (int)(GoodSync / (float)TotalSync * 100)
			: 0;

		return new TimerFrame()
		{
			Velocity = Owner.Velocity,
			Position = Owner.Position,
			Angles = (Owner as StrafePlayer).ViewAngles,
			Time = Timer,
			Jumps = Jumps,
			Strafes = Strafes,
			Sync = sync
		};
	}

}
