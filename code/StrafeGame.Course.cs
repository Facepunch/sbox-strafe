
using Sandbox;
using Strafe.Map;
using System.Linq;

namespace Strafe;

internal partial class StrafeGame
{

	[Net]
	public int StageCount { get; set; }
	[Net]
	public CourseTypes CourseType { get; set; }
	[Net]
	public string InvalidReason { get; set; }

	[Event.Entity.PostSpawn]
	private void SetupCourse()
	{
		if ( !IsServer ) return;

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

		if( stageStarts.Count() == 1 )
		{
			CourseType = CourseTypes.Linear;
			StageCount = All.OfType<LinearCheckpoint>().Count();
		}
		else
		{
			CourseType = CourseTypes.Staged;
			StageCount = stageStarts.Count();
		}
	}

	private void Invalidate( string reason )
	{
		InvalidReason = reason;
		CourseType = CourseTypes.Invalid;
		Log.Error( reason );
	}

}

public enum CourseTypes
{
	Invalid,
	Staged,
	Linear
}
