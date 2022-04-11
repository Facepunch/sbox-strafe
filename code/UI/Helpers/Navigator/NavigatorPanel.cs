using Sandbox;
using Sandbox.UI;
using Sandbox.Html;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Strafe.UI;

/// <summary>
/// A panel that acts like a website. A single page is always visible
/// but it will cache other views that you visit, and allow forward/backward navigation.
/// </summary>
[Library( "navigator" )]
public class NavigatorPanel : Panel
{
	public Panel CurrentPanel => Current?.Panel;
	public string CurrentUrl => Current?.Url;
	public string CurrentQuery;

	public Panel NavigatorCanvas { get; set; }

	protected class HistoryItem
	{
		public Panel Panel;
		public string Url;
	}

	internal void RemoveUrls( Func<string, bool> p )
	{
		var removes = Cache.Where( x => p( x.Url ) ).ToArray();

		foreach ( var remove in removes )
		{
			remove.Panel.Delete();
			Cache.Remove( remove );
		}
	}

	public override void OnTemplateSlot( INode element, string slotName, Panel panel )
	{
		if ( slotName == "navigator-canvas" )
		{
			NavigatorCanvas = panel;
			return;
		}

		base.OnTemplateSlot( element, slotName, panel );
	}

	protected List<HistoryItem> Cache = new();

	HistoryItem Current;
	Stack<HistoryItem> Back = new();
	Stack<HistoryItem> Forward = new();

	public void Navigate( string url )
	{
		var query = "";

		if ( url?.Contains( '?' ) ?? false )
		{
			var qi = url.IndexOf( '?' );
			query = url.Substring( qi + 1 );
			url = url.Substring( 0, qi );

			//Log.Info( $"Query: {query}" );
			//Log.Info( $"Url: {url}" );
		}

		//
		// Make url absolute by adding it to parent url
		//
		if ( url?.StartsWith( "~/" ) ?? false )
		{
			var parent = Ancestors.OfType<NavigatorPanel>().FirstOrDefault();
			if ( parent != null )
			{
				url = $"{parent.CurrentUrl}/{url[2..]}";
			}
		}

		if ( url == CurrentUrl )
		{
			ApplyQuery( query );
			return;
		}

		if ( NavigatorCanvas == null )
		{
			Log.Info( "Make Canvas This" );
			NavigatorCanvas = this;
		}

		var previousUrl = CurrentUrl;

		var attr = NavigatorTargetAttribute.FindValidTarget( url );
		if ( attr == null )
		{
			NotFound( url );
			return;
		}

		//Log.Info( $"{url} - {attr.FullName}" );

		Forward.Clear();

		if ( Current != null )
		{
			Back.Push( Current );
			Current.Panel.AddClass( "hidden" );
			Current = null;
		}

		var cached = Cache.FirstOrDefault( x => x.Url == url );
		if ( cached != null )
		{
			cached.Panel.RemoveClass( "hidden" );
			Current = cached;
			Current.Panel.Parent = NavigatorCanvas;
		}
		else
		{
			var panel = attr.Create<Panel>();
			panel.AddClass( "navigator-body" );

			Current = new HistoryItem { Panel = panel, Url = url };
			Current.Panel.Parent = NavigatorCanvas;

			foreach ( var (key, value) in attr.ExtractProperties( url ) )
			{
				panel.SetProperty( key, value );
			}

			Cache.Add( Current );
		}

		if ( Current == null ) return;

		Current.Panel.SetProperty( "referrer", previousUrl );
		ApplyQuery( query );
	}

	void ApplyQuery( string query )
	{
		if ( string.IsNullOrWhiteSpace( query ) )
			return;

		var parts = System.Web.HttpUtility.ParseQueryString( query );
		foreach ( var key in parts.AllKeys )
		{
			Current.Panel.SetProperty( key, parts.Get( key ) );
		}
	}

	protected virtual void NotFound( string url )
	{
		Log.Warning( $"Url Not Found: {url}" );
	}

	internal bool CurrentUrlMatches( string url )
	{
		if ( url != null && url.StartsWith( "~" ) )
			return CurrentUrl?.EndsWith( url[1..] ) ?? false;

		return CurrentUrl == url;
	}

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );


		if ( name == "default" ) Navigate( value );
	}

	/// <summary>
	/// Navigate to a URL
	/// </summary>
	[PanelEvent]
	public bool NavigateEvent( string url )
	{
		Navigate( url );
		return false;
	}


	/// <summary>
	/// 
	/// </summary>
	[PanelEvent( "navigate_return" )]
	public bool NavigateReturnEvent()
	{
		if ( !Back.TryPop( out var result ) )
			return true;

		Switch( result );
		return false;
	}

	protected override void OnBack( PanelEvent e )
	{
		if ( GoBack() )
		{
			e.StopPropagation();
		}
	}

	protected override void OnForward( PanelEvent e )
	{
		if ( GoForward() )
		{
			e.StopPropagation();
		}
	}

	public virtual bool GoBack()
	{
		if ( !Back.TryPop( out var result ) )
		{
			// TODO - only play this sound if we didn't pass to a parent
			PlaySound( "ui.navigate.deny" );
			return false;
		}

		if ( !Cache.Contains( result ) )
		{
			return GoBack();
		}

		PlaySound( "ui.navigate.back" );

		if ( Current != null )
			Forward.Push( Current );

		Switch( result );
		return true;
	}

	public virtual bool GoForward()
	{
		if ( !Forward.TryPop( out var result ) )
		{
			PlaySound( "ui.navigate.deny" );
			return false;
		}

		if ( !Cache.Contains( result ) )
		{
			return GoForward();
		}

		PlaySound( "ui.navigate.forward" );

		if ( Current != null )
			Back.Push( Current );

		Switch( result );
		return true;
	}

	void Switch( HistoryItem item )
	{
		if ( Current == item ) return;

		Current?.Panel.AddClass( "hidden" );
		Current = null;

		Current = item;
		Current?.Panel.RemoveClass( "hidden" );
	}
}
