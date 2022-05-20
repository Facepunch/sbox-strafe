
using Sandbox;
using Strafe.UI;
using System;
using System.Collections.Generic;

namespace Strafe.Menu;

internal partial class SlotMenu : Entity
{

	[Net]
	public string Title { get; set; }
	[Net]
	public List<string> Options { get; set; }
	[Net]
	public bool CloseButton { get; set; }
	[Net]
	public RealTimeUntil TimeUntilClose { get; set; }

	private List<Action<Client>> OptionActions = new();
	private SlotMenuHud Hud;

	public override void Spawn()
	{
		base.Spawn();

		TimeUntilClose = 30;

		Transmit = TransmitType.Always;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Hud = new SlotMenuHud( this );
		Local.Hud.AddChild( Hud );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		Hud?.Delete();
		Hud = null;
	}

	[Event.Tick.Server]
	private void OnTick()
	{
		if ( TimeUntilClose <= 0)
			Delete();
	}

	public void AddOption( string label, Action<Client> onChosen )
	{
		Host.AssertServer();

		Options.Add( label );
		OptionActions.Add( onChosen );
	}

	[ConCmd.Server]
	public static void ChooseOption( int menuIdent, int slot )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) return;

		var menu = Entity.FindByIndex( menuIdent ) as SlotMenu;
		if ( !menu.IsValid() ) return;

		if( slot < 0 || slot >= menu.Options.Count ) return;

		menu.OptionActions[slot]?.Invoke( ConsoleSystem.Caller );
	}

}
