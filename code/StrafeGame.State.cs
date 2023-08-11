
using Sandbox;
using Strafe.Menu;
using Strafe.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Strafe;

internal partial class StrafeGame
{

	[ConVar.Server("strafe_disable_mapcycle", Saved = true)]
	public static bool DisableMapCycle { get; set; }

	[Net]
	public RealTimeUntil StateTimer { get; set; }
	[Net]
	public string NextMap { get; set; }
	[Net]
	public IList<string> MapCycle { get; set; }
	[Net]
	public bool VoteFinalized { get; set; }
	[Net]
	public IDictionary<long, string> Nominations { get; set; }

	private MapVoteEntity MapVote;

	private async Task GameLoopAsync( float gametime = 1200f )
	{
		StateTimer = gametime;

		await RollMapCycle();

		while ( StateTimer > 0 )
		{
			if( DisableMapCycle )
			{
				await Task.DelayRealtimeSeconds( 1.0f );
				StateTimer = (int)(StateTimer + 1.5f);
				continue;
			}

			if ( (int)StateTimer == (7 * 60) )
			{
				DoMapVote();
			}

			if ( ShouldPrintTime() )
			{
				PrintTimeLeft();
			}

			await Task.DelayRealtimeSeconds( 1.0f );
		}

		Game.ChangeLevel( NextMap );
	}

	private bool ShouldPrintTime()
	{
		if ( (int)StateTimer % (5 * 60f) == 0 ) return true;
		if ( (int)StateTimer == 120 ) return true;
		if ( (int)StateTimer == 60 ) return true;
		if ( (int)StateTimer == 42 ) return true;
		if ( (int)StateTimer == 30 ) return true;
		if ( (int)StateTimer <= 10 ) return true;

		return false;
	}

	private void PrintTimeLeft()
	{
		Game.AssertServer();

		var tl = (int)StateTimer;
		if( tl > 60 )
		{
			var minutes = (int)(StateTimer / 60f);

			Chatbox.AddChatEntry( To.Everyone, "Server", $"{minutes} minutes remaining.", "info" );
		}
		else
		{
			Chatbox.AddChatEntry( To.Everyone, "Server", $"{tl} seconds remaining.", "info" );
		}
	}

	private async void DoMapVote( bool isFinal = false )
	{
		if ( MapVote.IsValid() ) return;
		if ( VoteFinalized ) return;

		var maplist = Nominations.Values.ToList();
		foreach( var m in MapCycle )
		{
			if ( maplist.Contains( m ) ) continue;
			maplist.Add( m );
		}
		maplist = maplist.Take( 7 ).ToList();

		MapVote = new MapVoteEntity( maplist );
		var result = await MapVote.DoVote();
		MapVote.Delete();
		MapVote = null;

		if ( result.StartsWith( "_extend" ) )
		{
			var extendMinutes = int.Parse( result.Replace( "_extend", "" ) );
			StateTimer += extendMinutes * 60f;

			Chatbox.AddChatEntry( To.Everyone, "Server", $"Map voting has ended, the current map has been extended {extendMinutes} minutes.", "info" );
		}
		else
		{
			NextMap = result;

			if ( isFinal )
			{
				VoteFinalized = true;
			}

			Chatbox.AddChatEntry( To.Everyone, "Server", $"Map voting has ended, the next map will be {NextMap}.", "info" );
		}
	}

	[ConCmd.Server]
	public static void Nominate( string ident )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) return;

		var playerid = ConsoleSystem.Caller.SteamId;

		if ( Current.Nominations.ContainsKey( playerid ) && Current.Nominations[playerid] == ident )
			return;

		Current.Nominations[playerid] = ident;
		Chatbox.AddChatEntry( To.Everyone, "Server", $"{ConsoleSystem.Caller.Name} has nominated {ident}", "server" );
	}

	public static async Task<List<string>> GetAvailableMaps()
	{
		var packages = await Package.FindAsync( "type:map game:facepunch.strafe sort:updated", 32 );
		var maps = packages.Packages?.Select( x => x.FullIdent ).ToList();

		var pkg = await Package.Fetch( Game.Server.GameIdent, true );
		if ( pkg != null )
		{
			maps.AddRange( pkg.GetMeta<List<string>>( "MapList", new() ) );
		}

		maps.RemoveAll( x => MapBlacklist.Contains( x.ToLower() ) );

		return maps;
	}

	static List<string> MapBlacklist = new()
	{
		"rival.surf_egypt" // csurf map
	};

	private void RockTheVote( IClient client )
	{
		if ( VoteFinalized )
		{
			Chatbox.AddChatEntry( To.Single( client ), "Server", $"Map voting is finished, the next map is {NextMap}", "info" );
			return;
		}

		client.SetValue( "rtv", true );

		var rtvcount = Game.Clients.Where( x => x.GetValue( "rtv", false ) == true ).Count();
		var totalcount = Game.Clients.Count;
		var needed = MathX.CeilToInt(totalcount / 2f);
		var remaining = Math.Max( 0, needed - rtvcount );

		Chatbox.AddChatEntry( To.Everyone, "Server", $"{client.Name} wants to rock the vote.  {remaining} votes remaining." );

		if ( remaining == 0 && !MapVote.IsValid() )
		{
			foreach( var c in Game.Clients )
			{
				c.SetValue( "rtv", false );
			}

			DoMapVote( true );
			StateTimer = 61f;
		}
	}

	private async Task RollMapCycle()
	{
		MapCycle = await GetAvailableMaps();
		MapCycle = MapCycle.OrderBy( x => Game.Random.Int( 9999 ) )
			.Distinct()
			.Where( x => x != Game.Server.MapIdent )
			.Take( 5 )
			.ToList();

		NextMap = Game.Random.FromList( MapCycle.ToList() );
		if ( string.IsNullOrEmpty( NextMap ) ) NextMap = Game.Server.MapIdent;
	}

}
