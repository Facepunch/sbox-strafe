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

	[ConVar.Server("strafe_disable_mapcycle")]
	public static bool DisableMapCycle { get; set; }

	[Net]
	public RealTimeUntil StateTimer { get; set; }
	[Net]
	public string NextMap { get; set; }
	[Net]
	public IDictionary<long, string> MapVotes { get; set; }

	private bool VotingFinished;
	private bool VoteInProgress;
	private List<string> MapCycle = new();

	private async Task GameLoopAsync( float gametime = 1800f )
	{
		StateTimer = gametime;
		MapCycle = await GetAvailableMaps();
		MapCycle = MapCycle.OrderBy( x => Rand.Int( 9999 ) )
			.Distinct()
			.Where( x => x != Global.MapName )
			.Take( 5 )
			.ToList();

		NextMap = Rand.FromList( MapCycle );
		if ( string.IsNullOrEmpty( NextMap ) ) NextMap = Global.MapName;

		while ( StateTimer > 0 )
		{
			if( DisableMapCycle )
			{
				await Task.DelayRealtimeSeconds( 1.0f );
				continue;
			}

			if ( (int)StateTimer == (7 * 60) )
			{
				_ = MapVoteAsync();
			}

			if ( ShouldPrintTime() )
			{
				PrintTimeLeft();
			}

			await Task.DelayRealtimeSeconds( 1.0f );
		}

		Global.ChangeLevel( NextMap );
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
		Host.AssertServer();

		var tl = (int)StateTimer;
		if( tl > 60 )
		{
			var minutes = (int)(StateTimer / 60f);

			Chat.AddChatEntry( To.Everyone, "Server", $"{minutes} minutes remaining.", "info" );
		}
		else
		{
			Chat.AddChatEntry( To.Everyone, "Server", $"{tl} seconds remaining.", "info" );
		}
	}

	private async Task MapVoteAsync()
	{
		Host.AssertServer();

		if ( VoteInProgress ) return;

		MapVotes.Clear();
		VotingFinished = false;
		VoteInProgress = true;

		Chat.AddChatEntry( To.Everyone, "Server", $"Map voting has started.", "info" );

		var menu = new SlotMenu();
		menu.Title = "Vote for the next map";

		foreach ( var m in MapCycle )
		{
			menu.AddOption( m, x => SetMapVote( x, m ) );
		}

		menu.AddOption( "Extend 15 minutes", x => SetMapVote( x, "_extend15" ) );

		while ( menu.IsValid() )
		{
			await Task.DelayRealtimeSeconds( 1.0f );
		}

		if ( NextMap.StartsWith( "_extend" ) )
		{
			var len = NextMap.Replace( "_extend", "" );
			var lenMins = int.Parse( len );
			StateTimer += lenMins * 60f;
			MapVotes.Clear();
			Chat.AddChatEntry( To.Everyone, "Server", $"Map voting has ended, the current map has been extended {len} minutes.", "info" );
		}
		else
		{
			VotingFinished = true;

			Chat.AddChatEntry( To.Everyone, "Server", $"Map voting has ended, the next map will be {NextMap}.", "info" );
		}

		VoteInProgress = false;
	}

	private void SetMapVote( Client client, string map )
	{
		if ( MapVotes.TryGetValue( client.PlayerId, out var vote ) && vote == map )
			return;

		MapVotes[client.PlayerId] = map;

		var votemap = new Dictionary<string, int>();
		MapVotes.Values.ToList().ForEach( x => votemap.Add( x, 0 ) );
		foreach ( var kvp in MapVotes )
		{
			votemap[kvp.Value]++;
		}
		NextMap = votemap.OrderByDescending( x => x.Value ).First().Key;

		if ( !map.StartsWith( "_extend" ) )
		{
			Chat.AddChatEntry( To.Everyone, "Server", $"{client.Name} voted for {map}", "info" );
		}
		else
		{
			var len = map.Replace( "_extend", "" );
			Chat.AddChatEntry( To.Everyone, "Server", $"{client.Name} voted to extend the map {len} minutes", "info" );
		}
	}

	private static async Task<List<string>> GetAvailableMaps()
	{
		var query = new Package.Query
		{
			Type = Package.Type.Map,
			Order = Package.Order.User,
			Take = 16,
		};

		query.Tags.Add( "game:facepunch.strafe" ); // maybe this should be a "for this game" type of thing instead

		var packages = await query.RunAsync( default );
		var maps = packages.Select( x => x.FullIdent ).ToList();

		var pkg = await Package.Fetch( Global.GameIdent, true );
		if ( pkg != null )
		{
			maps.AddRange( pkg.GetMeta<List<string>>( "MapList", new() ) );
		}

		return maps;
	}

	private void RockTheVote( Client client )
	{
		if ( VotingFinished || VoteInProgress ) return;

		client.SetValue( "rtv", true );

		var rtvcount = Client.All.Where( x => x.GetValue( "rtv", false ) == true ).Count();
		var totalcount = Client.All.Count;
		var needed = MathX.CeilToInt(totalcount / 2f);
		var remaining = Math.Max( 0, needed - rtvcount );

		Chat.AddChatEntry( To.Everyone, "Server", $"{client.Name} wants to rock the vote.  {remaining} votes remaining." );

		if ( remaining == 0 )
		{
			_ = MapVoteAsync();
			StateTimer = 61f;
		}
	}

}
