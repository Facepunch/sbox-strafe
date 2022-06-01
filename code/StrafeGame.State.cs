
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
	public IList<string> MapCycle { get; set; }
	[Net]
	public bool VoteFinalized { get; set; }

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

	private async void DoMapVote( bool isFinal = false )
	{
		if ( MapVote.IsValid() ) return;
		if ( VoteFinalized ) return;

		MapVote = new MapVoteEntity( MapCycle.ToList() );
		var result = await MapVote.DoVote();
		MapVote.Delete();
		MapVote = null;

		if ( result.StartsWith( "_extend" ) )
		{
			var extendMinutes = int.Parse( result.Replace( "_extend", "" ) );
			StateTimer += extendMinutes * 60f;

			Chat.AddChatEntry( To.Everyone, "Server", $"Map voting has ended, the current map has been extended {extendMinutes} minutes.", "info" );
		}
		else
		{
			NextMap = result;

			if ( isFinal )
			{
				VoteFinalized = true;
			}

			Chat.AddChatEntry( To.Everyone, "Server", $"Map voting has ended, the next map will be {NextMap}.", "info" );
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
		if ( VoteFinalized )
		{
			Chat.AddChatEntry( To.Single( client ), "Server", $"Map voting is finished, the next map is {NextMap}", "info" );
			return;
		}

		client.SetValue( "rtv", true );

		var rtvcount = Client.All.Where( x => x.GetValue( "rtv", false ) == true ).Count();
		var totalcount = Client.All.Count;
		var needed = MathX.CeilToInt(totalcount / 2f);
		var remaining = Math.Max( 0, needed - rtvcount );

		Chat.AddChatEntry( To.Everyone, "Server", $"{client.Name} wants to rock the vote.  {remaining} votes remaining." );

		if ( remaining == 0 && !MapVote.IsValid() )
		{
			foreach( var c in Client.All )
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
		MapCycle = MapCycle.OrderBy( x => Rand.Int( 9999 ) )
			.Distinct()
			.Where( x => x != Global.MapName )
			.Take( 5 )
			.ToList();

		NextMap = Rand.FromList( MapCycle.ToList() );
		if ( string.IsNullOrEmpty( NextMap ) ) NextMap = Global.MapName;
	}

}
