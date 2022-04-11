
using Sandbox;
using Sandbox.UI;

namespace Strafe.UI;

public struct GridLayout
{

	public GridLayout( Rect rect, int columns, int margin, int spacing, float scaleFromScreen )
	{
		Rect = rect;
		Columns = columns;
		Margin = margin;
		Spacing = spacing;
		ScaleFromScreen = scaleFromScreen;
	}

	public Rect Rect;
	public int Columns = 3;
	public int Margin = 0;
	public int Spacing = 0;
	public float ScaleFromScreen = 1;

	public void Position( int idx, Panel panel, int size = 0 )
	{
		var y = MathX.FloorToInt( idx / Columns );
		var x = idx - y * Columns;

		var margin = Margin * ScaleFromScreen;
		var spacing = Spacing * ScaleFromScreen;
		var sz = (Rect.width - spacing) / Columns * ScaleFromScreen;

		if ( size > 0 ) sz = size;

		panel.Style.Position = PositionMode.Absolute;
		panel.Style.Width = sz - spacing - margin * 2;
		panel.Style.Height = sz - spacing - margin * 2;
		panel.Style.Left = x * sz + spacing + margin;
		panel.Style.Top = y * sz + spacing + margin;
	}

	/// <summary>
	/// Returns -1 if outside
	/// </summary>
	/// <param name="localPosition">Position local to the rect</param>
	/// <returns></returns>
	public int SlotIndex( Vector2 localPosition )
	{
		if ( localPosition.x < 0 || localPosition.x > Rect.width ) return -1;
		if ( localPosition.y < 0 || localPosition.y > Rect.height ) return -1;

		localPosition *= ScaleFromScreen;

		var spacing = Spacing * ScaleFromScreen;
		var sz = (Rect.width - spacing) / Columns * ScaleFromScreen;

		var x = (int)(localPosition.x / sz);
		var y = (int)(localPosition.y / sz);

		return x + y * Columns;
	}

}
