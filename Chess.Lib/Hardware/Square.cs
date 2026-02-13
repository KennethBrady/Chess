using Chess.Lib.Hardware.Pieces;
using Common.Lib.Contracts;

namespace Chess.Lib.Hardware
{
	#region Interfaces

	public record struct PieceChange(IChessSquare Square, IChessPiece OldPiece, IChessPiece NewPiece);

	public interface IChessSquare
	{
		File File { get; }
		Rank Rank { get; }
		FileRank Position { get; }
		int Index { get; }
		Hue Hue { get; }
		IChessBoard Board { get; }
		IChessPiece Piece { get; }
		bool HasPiece => !(Piece is NoPiece);
		string Name => $"{File}{Rank.ToString()[1]}".ToLower();
		NeighborSquares Neighbors { get; }
		event Handler<PieceChange>? PieceChanged;
	}

	internal interface ISquare : IChessSquare
	{
		new IBoard Board { get; }
		new IPiece Piece { get; set; }
		void SetPiece(IPiece piece);
	}

	public interface INoSquare : IChessSquare;

	#endregion

	internal record struct NoSquare(IBoard Board, File File, Hue Hue, Rank Rank, int Index, IPiece Piece) : ISquare, INoSquare
	{
		internal static readonly NoSquare Default = new NoSquare(Hardware.Board.Default, File.Offboard, Hue.Default, Rank.Offboard, -1, NoPiece.Default);

		IChessBoard IChessSquare.Board => Board;
		IChessPiece IChessSquare.Piece => Piece;
		void ISquare.SetPiece(IPiece piece) { }
		string IChessSquare.Name => "Offboard";
		NeighborSquares IChessSquare.Neighbors => NeighborSquares.Empty;
		FileRank IChessSquare.Position => FileRank.OffBoard;
#pragma warning disable 00067
		public event Handler<PieceChange>? PieceChanged;
#pragma warning restore
	}

	// TODO: replace File and Rank with FileRank
	internal sealed record Square(IBoard Board, File File, Rank Rank, Hue Hue, int Index) : ISquare
	{
		private IPiece _piece = NoPiece.Default;
		IPiece ISquare.Piece
		{
			get => _piece;
			set
			{
				_piece = value;
			}
		}

		IChessBoard IChessSquare.Board => Board;
		IChessPiece IChessSquare.Piece => _piece;
		void ISquare.SetPiece(IPiece piece)
		{
			if (!ReferenceEquals(_piece, piece))
			{
				var old = _piece;
				_piece = piece;
				_piece.SetSquare(this);
				PieceChanged?.Invoke(new PieceChange(this, old, _piece));
			}
		}
		NeighborSquares IChessSquare.Neighbors => new NeighborSquares(this);
		public override string ToString() => ((ISquare)this).Name;
		public FileRank Position => new FileRank(File, Rank);
		public event Handler<PieceChange>? PieceChanged;
		public override int GetHashCode() => Index;
	}
}
