using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.UI.Images;
using Common.Lib.Contracts;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.MVVM;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Chess.Lib.UI.Dialogs
{
	public class BoardBuilderDialogModel : DialogModel<GameBoard>, IDialogTypeSpecifier
	{
		private List<PieceModel> _whitePieces, _blackPieces;
		private readonly List<SquareModel> _squares;
		private bool _isEraserActive, _verifyBoardValidity;
		private Hue _nextMove = Hue.White;
		private string _fen = string.Empty;
		public BoardBuilderDialogModel(IChessBoard startBoard)
		{
			_whitePieces = PieceTypeExtensions.AllValid.Select(t => new PieceModel(this, t, Hue.White)).ToList();
			_blackPieces = PieceTypeExtensions.AllValid.Select(t => new PieceModel(this, t, Hue.Black)).ToList();
			_squares = startBoard.Select(s => new SquareModel(this, s)).Chunk(8).Reverse().SelectMany(s => s).ToList();
		}
		public BoardBuilderDialogModel() : this(GameFactory.CreateBoard(false)) { }

		public IEnumerable<SquareModel> Squares => _squares;

		public IEnumerable<PieceModel> WhitePieces => _whitePieces.Chunk(2).Reverse().SelectMany(s => s);
		public IReadOnlyList<PieceModel> BlackPieces => _blackPieces.AsReadOnly();

		public bool IsEraserActive
		{
			get => _isEraserActive;
			set
			{
				_isEraserActive = value;
				Notify(nameof(IsEraserActive));
				ApplyEraser();
			}
		}

		public Hue NextMove
		{
			get => _nextMove;
			set
			{
				_nextMove = value;
				Notify(nameof(NextMove));
			}
		}

		public bool HasPieces => _squares.Any(s => s.HasPiece);

		public bool IsBoardEmpty => !_squares.Any(s => s.HasPiece);

		public bool VerifyBoardValidity
		{
			get => _verifyBoardValidity;
			set
			{
				_verifyBoardValidity = value;
				Notify(nameof(VerifyBoardValidity));
				RaiseCanExecuteChanged();
			}
		}

		public bool IsBoardValid { get; private set; } = true;

		public string Fen
		{
			get => _fen;
			set
			{
				_fen = value;
				Notify(nameof(Fen), nameof(FenLabel));
			}
		}

		public string FenLabel => string.Equals(BoardFen, _fen) ? "Copy FEN" : "Apply FEN";

		Type IDialogTypeSpecifier.DialogType => typeof(BoardBuilderDialog);

		protected override bool CanExecute(string? parameter)
		{
			switch (parameter)
			{
				case OKParameter:
					if (IsBoardEmpty) return false;
					return !_verifyBoardValidity || IsBoardValid;
				case CancelParameter: return true;
				case "setClassical": return true;
				case "fen": return FEN.IsValidPiecePlacements(_fen);
				case "clear": return !IsBoardEmpty;
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case OKParameter: Accept(CreateBoard()); break;
				case CancelParameter: Cancel(); break;
				case "setClassical": SetClassical(); break;
				case "clear": ClearAllPieces(); break;
				case "fen": ApplyFen(); break;
			}
		}

		private string BoardFen { get; set; } = string.Empty;
		private void ApplyFen()
		{
			if (_fen == BoardFen)
			{
				Clipboard.SetText(_fen);
				// TODO: set status
			}
			else
			{
				FEN fen = FEN.Parse(_fen);
				if (!fen.IsEmpty) ApplyBoard(fen.ToBoard());
			}
		}

		private void SetClassical() => ApplyBoard(GameFactory.CreateBoard(true));

		private void ClearAllPieces()
		{
			_squares.ForEach(s => s.Clear());
			foreach (var pm in AllPieceModels) pm.UseCount = 0;
			Notify(nameof(HasPieces));
		}

		private GameBoard CreateBoard()
		{
			BoardBuilder bb = new BoardBuilder();
			foreach (SquareModel m in _squares.Where(s => !s.AppliedPiece.IsDefault))
			{
				bb.SetPiece(m.Square.Position, m.AppliedPiece.Type, m.AppliedPiece.Hue);
			}
			return GameBoard.Default with { Board = bb.CreateBoard(), Type = GameBoardType.Custom, NextMove = _nextMove };
		}

		private void ApplyBoard(IChessBoard board)
		{
			Dictionary<int, SquareModel> squares = _squares.ToDictionary(s => s.Square.Index);
			foreach (IChessSquare sq in board) squares[sq.Index].SetPiece(sq.Piece);
			BoardFen = _fen = board.FENPiecePlacements;
			Notify(nameof(HasPieces), nameof(Fen), nameof(FenLabel));
		}

		private IEnumerable<PieceModel> AllPieceModels => _whitePieces.Concat(_blackPieces);

		private void UpdatePieceCounts(bool clearCursor)
		{
			if (clearCursor && _isEraserActive) SetPieceCursor(PieceDef.Default);
			Dictionary<PieceDef, int> counts = new();
			foreach (PieceType p in PieceTypeExtensions.AllValid)
			{
				counts.Add(new PieceDef(p, Hue.White), 0);
				counts.Add(new PieceDef(p, Hue.Black), 0);
			}
			foreach (SquareModel sm in _squares)
			{
				if (!sm.AppliedPiece.IsDefault) counts[sm.AppliedPiece]++;
			}
			foreach (PieceModel pm in _blackPieces) pm.UseCount = counts[new PieceDef(pm.Type, Hue.Black)];
			foreach (PieceModel pm in _whitePieces) pm.UseCount = counts[new PieceDef(pm.Type, Hue.White)];
			RaiseCanExecuteChanged();
			Notify(nameof(HasPieces));
			if (HasPieces == false && IsEraserActive) IsEraserActive = false;
			if (_verifyBoardValidity) VerifyValidity();
		}

		protected override void HandleEscapeKey()
		{
			if (_isEraserActive)
			{
				IsEraserActive = false;
				return;
			}
			PieceModel? m = AllPieceModels.FirstOrDefault(m => m.IsLocked);
			if (m != null)
			{
				m.ToggleLocked();
				SetPieceCursor(PieceDef.Default);
			}
			else base.HandleEscapeKey();
		}

		private void VerifyValidity()
		{
			BoardInfo info = new BoardInfo(CreateBoard().Board);
			if (!info.HasBothKings || !info.IsMatePossible) IsBoardValid = false; else IsBoardValid = true;
			Notify(nameof(IsBoardValid));
		}

		#region Cursor Management

		/*	Cursor Management
		 * 
		 *	Default cursor is arrow.
		 *	Cursor is set to a piece when the piece is locked.
		 *	Cursor is set to eraser when eraser is toggled.
		 *	
		 *	Clicking on a square with the piece cursor applies the locked piece to that square.
		 *	Dragging a piece to a square places the piece on the square and clears the cursor, locked piece, and/or eraser.
		 */


		internal Action<PieceDef> SetPieceCursor { get; set; } = Actions<PieceDef>.Empty;
		internal Action SetEraserCursor { get; set; } = Actions.Empty;

		private PieceModel? LockedPiece => AllPieceModels.FirstOrDefault(m => m.IsLocked);

		private void SetLockedPiece(PieceModel locked)
		{
			_isEraserActive = false;
			Notify(nameof(IsEraserActive));
			foreach (PieceModel m in _whitePieces.Concat(_blackPieces))
			{
				if (ReferenceEquals(m, locked)) continue;
				if (m.IsLocked)
				{
					m.ToggleLocked();
					break;
				}
			}
			SetPieceCursor(new PieceDef(locked.Type, locked.Hue));
		}

		private void ClearLockedPiece()
		{
			PieceModel? m = AllPieceModels.FirstOrDefault(m => m.IsLocked);
			if (m != null) m.ToggleLocked();
			ClearCursor();
		}

		private void ApplyEraser()
		{
			if (_isEraserActive)
			{
				var locked = AllPieceModels.FirstOrDefault(m => m.IsLocked);
				if (locked != null) locked.ToggleLocked();
				SetEraserCursor();
			}
			else ClearCursor();
		}

		private void ClearCursor() => SetPieceCursor(PieceDef.Default);

		#endregion

		#region PieceModel

		[DebuggerDisplay("{Hue} {Type}")]
		public class PieceModel : ViewModel
		{
			private bool _locked;
			private int _boardCount;
			internal PieceModel(BoardBuilderDialogModel owner, PieceType type, Hue hue)
			{
				Owner = owner;
				Type = type;
				Hue = hue;
			}

			private BoardBuilderDialogModel Owner { get; init; }
			public PieceType Type { get; private init; }
			public Hue Hue { get; private init; }

			public bool IsLocked => _locked;

			public ImageSource Piece => ImageLoader.LoadImage(Type, Hue);

			public Brush LockedBorder => IsLocked ? Brushes.Yellow : Brushes.Transparent;
			public int UseCount
			{
				get => _boardCount;
				set
				{
					if (_boardCount != value)
					{
						_boardCount = value;
						Notify(nameof(UseCount));
					}
				}
			}

			internal void ToggleLocked()
			{
				_locked = !_locked;
				Notify(nameof(IsLocked), nameof(LockedBorder));
				if (_locked) Owner.SetLockedPiece(this);
			}
		}

		#endregion

		#region SquareModel

		[DebuggerDisplay("{Square.Position}: {AppliedPiece}")]
		public class SquareModel : ViewModel
		{
			internal SquareModel(BoardBuilderDialogModel owner, IChessSquare square)
			{
				Owner = owner;
				Square = square;
				AppliedPiece = square.HasPiece ? square.Piece.Definition : PieceDef.Default;
				Piece = Square.Piece is INoPiece ? null : ImageLoader.LoadImage(Square.Piece);
			}

			private BoardBuilderDialogModel Owner { get; init; }

			public Hue Hue => Square.Hue;

			public Brush Fill => Hue == Hue.White ? ChessBoardProperties.LightSquareBrush : ChessBoardProperties.DarkSquareBrush;

			public ImageSource? Piece { get; private set; }

			public IChessSquare Square { get; private init; }

			public PieceDef AppliedPiece { get; private set; } = PieceDef.Default;

			public bool HasPiece => !AppliedPiece.IsDefault;

			// Called when a piece is dragged from one square to another
			internal void SetPieceFromSquare(SquareModel source)
			{
				Piece = source.Piece;
				Notify(nameof(Piece));
				source.Piece = null;
				source.Notify(nameof(Piece));
				Owner.UpdatePieceCounts(true);
			}

			// Called by the owner model to apply classical or Chess960 boards
			internal void SetPiece(IChessPiece piece)
			{
				if (piece is INoPiece)
				{
					AppliedPiece = PieceDef.Default;
					Piece = null;
				}
				else
				{
					AppliedPiece = new PieceDef(piece.Type, piece.Side);
					Piece = ImageLoader.LoadImage(AppliedPiece);
				}
				Notify(nameof(Piece));
				Owner.RaiseCanExecuteChanged();
			}

			// Called when a PieceModel is dropped onto a square (clearCursor = true)
			// Called 
			internal void SetPieceFromModel(PieceModel? piece, bool clearCursor)
			{
				if (piece == null)
				{
					Piece = null;
					AppliedPiece = PieceDef.Default;
				}
				else
				{
					Piece = piece.Piece;
					AppliedPiece = new PieceDef(piece.Type, piece.Hue);
				}
				Notify(nameof(Piece));
				Owner.UpdatePieceCounts(piece != null);
				if (clearCursor) Owner.ClearLockedPiece();
			}


			// Called when the user clicks on the grid.
			internal bool ApplyGridClick()
			{
				if (Owner.IsEraserActive) Clear();
				else
				{
					var locked = Owner.LockedPiece;
					if (locked == null) return false;
					SetPieceFromModel(locked, false);
				}
				return true;
			}

			internal void Clear()
			{
				if (!AppliedPiece.IsDefault)
				{
					AppliedPiece = PieceDef.Default;
					Piece = null;
					Notify(nameof(Piece));
					Owner.UpdatePieceCounts(false);
				}
			}
		}

		#endregion
	}
}
