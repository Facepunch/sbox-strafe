using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Surf;

namespace Strafe;

partial class StrafeGame
{
	[ConVar.Replicated]
	public static string strafe_map_asset { get; set; }

	[Net]
	public SurfMapAsset ProcSurfMapAsset { get; set; }

	public SurfMap ProcSurfMap { get; set; }

	private int _procSurfMapChangeIndex;

	private void ClearProcSurfCourse()
	{
		ProcSurfMap?.Clear();
		ProcSurfMap = null;
		ProcSurfMapAsset = null;
		_procSurfMapChangeIndex = 0;
	}

	private void SetupProcSurfCourse()
	{
		Log.Info( $"Asset Path: {strafe_map_asset}" );
		ProcSurfMapAsset = ResourceLibrary.Get<SurfMapAsset>( strafe_map_asset );

		foreach ( var spawn in All.OfType<SpawnPoint>().ToArray() )
		{
			spawn.Delete();
		}

		foreach ( var platform in ProcSurfMapAsset.SpawnPlatforms )
		{
			var rotation = Rotation.FromYaw( platform.Yaw );

			if ( platform.Stage == 1 )
			{
				for ( var i = 0; i < 4; ++i )
				for ( var j = 0; j < 4; ++j )
				{
					_ = new SpawnPoint
					{
						Position = platform.Position + Vector3.Up * 64f + rotation.Right * (i - 1.5f) * 64f + rotation.Forward * (j - 1.5f) * 64f,
						Rotation = rotation
					};
				}
			}

			var start = new StageStart
			{
				Position = platform.Position,
				Rotation = rotation,
				Stage = platform.Stage
			};

			start.SetupPhysicsFromAABB( PhysicsMotionType.Static, new Vector3( -256f, -256f, 0f ),
				new Vector3( 256f, 256f, 128f ) );
		}

		foreach ( var checkpoint in ProcSurfMapAsset.Checkpoints )
		{
			var finish = new StageEnd
			{
				Position = checkpoint.Position,
				Rotation = Rotation.From( checkpoint.Angles ),
				Stage = checkpoint.Stage
			};

			finish.SetupPhysicsFromOBB( PhysicsMotionType.Static, new Vector3( -8f, -256f, -256f ),
				new Vector3( 8f, 256f, 256f ) );
		}

		var respawn = new StrafeTriggerTeleportProgress
		{
			Position = Vector3.Zero,
			SetViewAngles = true
		};

		respawn.SetupPhysicsFromAABB( PhysicsMotionType.Static, new Vector3( -16384f, -16384f, 0f ),
			new Vector3( 16384f, 16384f, 512f ) );
	}

	[GameEvent.Tick.Server, GameEvent.Tick.Client]
	private void ProcSurfTickShared()
	{
		if ( ProcSurfMapAsset == null )
		{
			return;
		}

		if ( ProcSurfMap == null )
		{
			ProcSurfMap = new SurfMap( Game.SceneWorld, Game.PhysicsWorld.Body );
			_procSurfMapChangeIndex = -1;
		}

		if ( _procSurfMapChangeIndex != ProcSurfMapAsset.ChangeIndex )
		{
			_procSurfMapChangeIndex = ProcSurfMapAsset.ChangeIndex;
			ProcSurfMap.Load( ProcSurfMapAsset );
		}

		ProcSurfMap.UpdateChangedElements();
	}
}
