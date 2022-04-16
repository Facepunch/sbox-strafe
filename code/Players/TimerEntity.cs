
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

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Owner;
	}

	public void Start()
	{
		Timer = 0f;
		Checkpoint = 0;
		State = States.Live;
	}

	public void Stop()
	{
		Timer = 0f;
		Checkpoint = 0;
		State = States.Stopped;
	}

	public void Complete()
	{
		State = States.Complete;

		if ( IsServer )
		{
			var thing = Stage == 0
				? "the course"
				: $"stage {Stage}";

			Chat.AddChatEntry( To.Everyone, "Server", $"{Owner.Client.Name} finished {thing} in {Timer.HumanReadable()}s" );
		}
	}

	public void SetCheckpoint( int checkpoint )
	{
		if ( checkpoint <= Checkpoint ) 
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

		if ( State != States.Live ) 
			return;

		Timer += Time.Delta;
	}

	public void TeleportTo()
	{
		if ( Owner is not StrafePlayer pl ) 
			return;

		var start = All.First( x => x is StageStart s && s.Stage == Stage );
		pl.Position = start.Position;
		pl.Rotation = start.Rotation;
	}

}
