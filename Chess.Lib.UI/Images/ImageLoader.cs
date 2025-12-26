using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chess.Lib.UI.Images
{
	internal static class ImageLoader
	{
		internal static ImageSource? LoadImage(IChessPiece piece)
		{
			if (piece is null) return null;
			if (piece.Type == PieceType.None) return null;
			return LoadImage(piece.Type, piece.Side);
		}

		internal static ImageSource LoadImage(PieceType type, Hue side)
		{
			string name = side == Hue.Light ? "White" : "Black";
			name += type.ToString();
			Uri uri = new Uri($"Chess.Lib.UI;component/Images/{name}.png", UriKind.Relative);
			var info = Application.GetResourceStream(uri);
			PngBitmapDecoder dec = new PngBitmapDecoder(info.Stream, BitmapCreateOptions.None, BitmapCacheOption.None);
			return dec.Frames[0];
		}
	}
}
