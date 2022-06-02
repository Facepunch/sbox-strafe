
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Strafe.UI.Components;

[Alias( "split-label", "label2" )]
internal class SplitLabel : Panel
{

	public Label Left { get; set; }
	public Label Right { get; set; }

	public SplitLabel()
	{
		Left = Add.Label( "left" );
		Right = Add.Label( "right" );
	}

	public override void SetProperty( string name, string value )
	{
		switch ( name )
		{
			case "left":
				Left.Text = value;
				break;
			case "right":
				Right.Text = value;
				break;
			default:
				base.SetProperty( name, value );
				break;
		}
	}

}
