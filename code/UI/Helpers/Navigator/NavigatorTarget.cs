using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Strafe.UI;

/// <summary>
/// Mark a Panel with this class for it to be navigatable
/// </summary>
public class NavigatorTargetAttribute : LibraryAttribute
{
	public string Url { get; internal set; }

	string[] Parts;

	public NavigatorTargetAttribute( string url )
	{
		Url = url;
		Parts = url.ToString().Split( '/', StringSplitOptions.RemoveEmptyEntries );
	}

	bool TestPart( string part, string ours )
	{
		// this is a variable
		if ( ours != null && ours.StartsWith( '{' ) && ours.EndsWith( '}' ) )
			return true;

		return part == ours;
	}

	public bool CanServeUrl( string url )
	{
		if ( string.IsNullOrEmpty( url ) ) return false;

		if ( url.Contains( '?' ) )
		{
			url = url[..url.IndexOf( '?' )];
		}

		var a = url.ToString().Split( '/', StringSplitOptions.RemoveEmptyEntries );

		for ( int i = 0; i < Parts.Length || i < a.Length; i++ )
		{
			var left = i < a.Length ? a[i] : null;
			var right = i < Parts.Length ? Parts[i] : null;

			if ( !TestPart( left, right ) )
				return false;
		}

		return true;
	}

	public static NavigatorTargetAttribute FindValidTarget( string url )
	{
		return Library.GetAttributes<NavigatorTargetAttribute>()
			.Where( x => x.CanServeUrl( url ) )
			.FirstOrDefault();
	}

	internal IEnumerable<(string key, string value)> ExtractProperties( string url )
	{
		var a = url.ToString().Split( '/', StringSplitOptions.RemoveEmptyEntries );

		for ( int i = 0; i < Parts.Length; i++ )
		{
			if ( !Parts[i].StartsWith( '{' ) ) continue;
			if ( !Parts[i].EndsWith( '}' ) ) continue;

			var key = Parts[i][1..^1].Trim( '?' );

			if ( i < a.Length )
			{
				yield return (key, a[i]);
			}
			else
			{
				yield return (key, null);
			}
		}
	}
}
