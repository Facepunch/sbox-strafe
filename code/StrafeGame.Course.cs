
using Sandbox;
using Strafe.Api;
using Strafe.Leaderboards;
using Strafe.Map;
using System.Linq;
using System.Threading.Tasks;

namespace Strafe;

internal partial class StrafeGame
{

	[Net]
	public int StageCount { get; set; }
	[Net]
	public CourseTypes CourseType { get; set; }
	[Net]
	public string InvalidReason { get; set; }

	[GameEvent.Entity.PostSpawn]
	private void SetupCourse()
	{
		if ( !Game.IsServer ) return;

		var config = All.OfType<StrafeMapConfig>().FirstOrDefault();

		if ( config is { ProceduralArena: true } )
		{
			SetupProcSurfCourse();
		}
		else
		{
			ClearProcSurfCourse();
		}

		var stageStarts = All.OfType<StageStart>();
		if( !stageStarts.Any() )
		{
			Invalidate( "No stage zones exist." );
			return;
		}

		foreach( var start in stageStarts )
		{
			var endzone = All.OfType<StageEnd>().FirstOrDefault( x => x.Stage == start.Stage );
			if ( !endzone.IsValid() )
			{
				Invalidate( $"Stage {start.Stage} is missing an end zone." );
				return;
			}
		}

		var stageEnds = All.OfType<StageEnd>();
		foreach( var end in stageEnds )
		{
			var startzone = All.OfType<StageStart>().FirstOrDefault( x => x.Stage == end.Stage );
			if ( !startzone.IsValid() )
			{
				Invalidate( $"Stage {end.Stage} is missing a start zone." );
				return;
			}
		}

		if( stageStarts.Count() == 1 && All.OfType<LinearCheckpoint>().Any() )
		{
			CourseType = CourseTypes.Linear;
			StageCount = All.OfType<LinearCheckpoint>().Count();
		}
		else
		{
			CourseType = CourseTypes.Staged;
			StageCount = stageStarts.Count();
		}

		NetworkServerLogin();
	}

	private void Invalidate( string reason )
	{
		InvalidReason = reason;
		CourseType = CourseTypes.Invalid;
		Log.Error( reason );
	}

	private Dictionary<TimerStyles, ReplayEntity> WrReplays = new();
	private async Task DownloadWrReplay()
	{
		foreach( TimerStyles style in Enum.GetValues<TimerStyles>() )
		{
			var replay = await Backend.FetchReplay( Game.Server.MapIdent, 0, 1, style );
			if ( replay == null ) continue;

			var playerName = await Backend.FetchPlayerName( replay.PlayerId );

			SetWrReplay( replay, style, playerName );
		}
	}

	public void SetWrReplay( Replay replay, TimerStyles style, string playerName = null )
	{
		if ( WrReplays.ContainsKey( style ) && WrReplays[style].IsValid() )
		{
			WrReplays[style].Delete();
			WrReplays.Remove( style );
		}

		WrReplays[style] = ReplayEntity.Play( replay, -1, style, playerName );
	}

}

public enum CourseTypes
{
	Invalid,
	Staged,
	Linear,
	StagedLinear
}
