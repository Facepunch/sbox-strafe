
using Sandbox;
using Strafe.Players;
using System;
using System.Collections.Generic;
using System.IO;

namespace Strafe.Leaderboards;

internal class Replay
{

	public Replay( long playerid, List<TimerFrame> frames ) 
	{
		Frames = frames;
		PlayerId = playerid;
		MapIdent = Global.MapName;
		Date = DateTimeOffset.UtcNow;
	}

	public int Version { get; set; } = 1;
	public long PlayerId { get; set; }
	public string MapIdent { get; set; }
	public DateTimeOffset Date { get; set; }
	public IReadOnlyList<TimerFrame> Frames { get; set; }

	public byte[] ToBytes() => ToBytes( this );
	public static byte[] ToBytes( Replay replay )
	{
		Assert.NotNull( replay?.Frames );

		using var ms = new MemoryStream();
		using var bw = new BinaryWriter( ms );

		bw.Write( replay.Version );
		bw.Write( replay.PlayerId );
		bw.Write( replay.MapIdent );
		bw.Write( replay.Date.ToString( "o" ) );
		bw.Write( replay.Frames.Count );

		for( int i = 0; i < replay.Frames.Count; i++ )
		{
			bw.WriteTimerFrame( replay.Frames[i], replay.Version );
		}

		return ms.ToArray();
	}

	public static Replay FromBytes( byte[] data )
	{
		Assert.NotNull( data );

		using var ms = new MemoryStream( data );
		using var br = new BinaryReader( ms );

		var version = br.ReadInt32();
		var playerid = br.ReadInt64();
		var mapIdent = br.ReadString();
		var date = DateTimeOffset.Parse( br.ReadString() );
		var frameCount = br.ReadInt32();

		var frames = new List<TimerFrame>( frameCount );
		var result = new Replay( playerid, frames ) { Version = version };

		for( int i = 0; i < frameCount; i++ )
		{
			frames.Add( br.ReadTimerFrame( version ) );
		}

		return result;
	}

}

internal static class ReplayIO
{

	internal static TimerFrame ReadTimerFrame( this BinaryReader br, int version )
	{
		return new TimerFrame()
		{
			Velocity = br.ReadVector3(),
			Position = br.ReadVector3(),
			Angles = br.ReadAngles(),
			Time = br.ReadSingle(),
			Jumps = br.ReadUInt16(),
			Strafes = br.ReadUInt16(),
		};
	}

	internal static void WriteTimerFrame( this BinaryWriter bw, TimerFrame frame, int version )
	{
		bw.Write( frame.Velocity );
		bw.Write( frame.Position );
		bw.Write( frame.Angles );
		bw.Write( frame.Time );
		bw.Write( frame.Jumps );
		bw.Write( frame.Strafes );
	}

}
