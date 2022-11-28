
using Sandbox;
using Strafe.Menu;
using Strafe.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Strafe;

internal partial class MapVoteEntity : Entity
{

	[Net]
	public IDictionary<long, string> MapVotes { get; set; }
	[Net]
	public IList<string> MapCycle { get; set; }
	[Net]
	public string Outcome { get; set; }

	public MapVoteEntity( List<string> maps )
	{
		MapCycle = maps;
		Outcome = Rand.FromList( maps );

		Transmit = TransmitType.Never;
	}

	public async Task<string> DoVote()
	{
		var menu = new SlotMenu( 20 );
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

		if ( string.IsNullOrEmpty( Outcome ) )
		{
			Log.Error( "Map vote ended up null or empty, defaulting to current map" );
			Outcome = Global.MapName;
		}

		return Outcome;
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
		Outcome = votemap.OrderByDescending( x => x.Value ).First().Key;

		if ( !map.StartsWith( "_extend" ) )
		{
			Chatbox.AddChatEntry( To.Everyone, "Server", $"{client.Name} voted for {map}", "info" );
		}
		else
		{
			var len = map.Replace( "_extend", "" );
			Chatbox.AddChatEntry( To.Everyone, "Server", $"{client.Name} voted to extend the map {len} minutes", "info" );
		}
	}

}
