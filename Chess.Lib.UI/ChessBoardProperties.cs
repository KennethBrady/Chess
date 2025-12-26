using Chess.Lib.Hardware;
using Common.Lib.Contracts;
using System.Windows.Media;

namespace Chess.Lib.UI;

internal record struct BrushChange(Hue Hue, Brush Brush);
public static class ChessBoardProperties
{
	private static Brush _lightBrush = Brushes.White;
	private static Brush _darkBrush = Brushes.DarkGray;
	private static Brush _moveTargetBrush = Brushes.Lime;
	private static Brush _lastMoveBrush = new SolidColorBrush(Color.FromArgb(127, 0, 127, 0));
	private static Brush _checkBrush = new SolidColorBrush(Color.FromArgb(127, 0xff, 0, 0));

	internal static event TypeHandler<BrushChange>? BrushChanged;

	public static Brush LightSquareBrush
	{
		get => _lightBrush;
		set
		{
			_lightBrush = value;
			BrushChanged?.Invoke(new BrushChange(Hue.Light, _lightBrush));
		}
	}

	public static Brush DarkSquareBrush
	{
		get => _darkBrush;
		set
		{
			_darkBrush = value;
			BrushChanged?.Invoke(new BrushChange(Hue.Dark, _darkBrush));
		}
	}

	public static Brush MoveTargetColor
	{
		get => _moveTargetBrush;
		set => _moveTargetBrush = value;
	}

	public static Brush LastMoveBrush
	{
		get => _lastMoveBrush;
	}

	public static Brush CheckedKingBrush => _checkBrush;
}
