
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Strafe.Menu;
using System;

namespace Strafe.UI;

[UseTemplate]
internal class SlotMenuHud : Panel
{

	private SlotMenu Menu;
	private int Hash;

	public string Title => Menu.Title;
	public string Duration => ((int)Menu.TimeUntilClose).ToString();
	public Panel OptionsCanvas { get; set; }

	public SlotMenuHud( SlotMenu menu )
	{
		Menu = menu;
	}

	public override void Tick()
	{
		base.Tick();

		var hashicorp = Menu.CloseButton.GetHashCode();
		foreach( var option in Menu.Options )
		{
			hashicorp = HashCode.Combine( hashicorp, option.GetHashCode() );
		}

		if ( Hash == hashicorp ) return;
		Hash = hashicorp;

		RebuildOptions();
	}

	private void RebuildOptions()
	{
		foreach( var option in Menu.Options )
		{
			OptionsCanvas.Add.Label( $"{Menu.Options.IndexOf(option) + 1}. {option}" );
		}

		OptionsCanvas.Add.Panel().Style.Height = Length.Pixels( 16 );
		OptionsCanvas.Add.Label( $"0. Close" );
	}

	private void SubmitOption( int slot )
	{
		if ( slot < -1 || slot >= Menu.Options.Count ) return;

		if( slot > -1 )
		{
			SlotMenu.ChooseOption( Menu.NetworkIdent, slot );
		}

		Delete();
	}

	[Event.BuildInput]
	public void OnBuildInput( InputBuilder b )
	{
		for( int i = 0; i <= 9; i++ )
		{
			var e = Enum.Parse<InputButton>( $"Slot{i}" );
			if ( b.Pressed( e ) ) SubmitOption( i - 1 );
		}
	}

}
