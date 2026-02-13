using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using Chess.Lib.UI.Images;
using Common.Lib.UI.Dialogs;
using System.Windows;
using System.Windows.Media;

namespace Chess.Lib.UI.Dialogs
{
	/// <summary>
	/// Interaction logic for PromotionDialog.xaml
	/// </summary>
	public partial class PromotionDialog : DialogView
	{
		public PromotionDialog()
		{
			InitializeComponent();
		}

		protected override Point CalculateInitialPosition()
		{
			if (DataContext is PromotionDialogModel m)
			{
				//return m.Square.PointToScreen(new Point());
				return m.MousePosition;
			}
			return base.CalculateInitialPosition();
		}
	}

	internal class PromotionDialogModel : DialogModel<Promotion>, IDialogTypeSpecifier
	{
		internal PromotionDialogModel(Promotion promotion, Point mpos)
		{
			Promotion = promotion;
			MousePosition = mpos;
		}

		public Hue Hue => Promotion.Hue;
		public IChessSquare OnSquare => Promotion.OnSquare;

		public Point MousePosition { get; private init; }

		public ImageSource Knight => ImageLoader.LoadImage(PieceType.Knight, Hue);
		public ImageSource Bishop => ImageLoader.LoadImage(PieceType.Bishop, Hue);
		public ImageSource Rook => ImageLoader.LoadImage(PieceType.Rook, Hue);
		public ImageSource Queen => ImageLoader.LoadImage(PieceType.Queen, Hue);

		Type IDialogTypeSpecifier.DialogType => typeof(PromotionDialog);

		protected override bool CanExecute(string? parameter) => true;

		protected override void Execute(string? parameter)
		{
			PieceType pt = PieceType.None;
			switch (parameter)
			{
				case "knight": pt = PieceType.Knight; break;
				case "bishop": pt = PieceType.Bishop; break;
				case "rook": pt = PieceType.Rook; break;
				case "queen": pt = PieceType.Queen; break;
			}
			Accept(Promotion with { PieceType = pt });
		}

		protected override void HandleEscapeKey()
		{
			// Not allowed
		}

		internal Promotion Promotion { get; private init; }
	}
}
