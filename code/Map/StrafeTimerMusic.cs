
namespace Strafe;

[Library( "strafe_timer_music" ), HammerEntity]
[EditorSprite( "editor/snd_event.vmat" ), VisGroup( VisGroup.Sound )]
[Title( "Timer Music" ), Category( "Sound" ), Icon( "volume_up" )]
public partial class StrafeTimerMusic : Entity
{

	/// <summary>
	/// Name of the sound to play.
	/// </summary>
	[Property( "soundName" ), FGDType( "sound" )]
	[Net] public string SoundName { get; set; }

	Players.TimerEntity.States LastState;
	Sound SoundEvent;
	
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	[Event.Tick.Client]
	public void OnTick()
	{
		if ( Game.LocalPawn is not StrafePlayer pl ) return;
		if ( pl.TimerState == LastState ) return;

		LastState = pl.TimerState;
		OnTimerStateChanged( LastState, pl.TimerState );
	}

	void OnTimerStateChanged( Players.TimerEntity.States oldState, Players.TimerEntity.States newState )
	{
		if( newState != Players.TimerEntity.States.Live )
		{
			SoundEvent.Stop();
		}
		else
		{
			SoundEvent = Sound.FromWorld( SoundName, Position );
		}
	}

}
