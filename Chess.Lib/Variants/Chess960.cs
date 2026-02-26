using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.Variants
{
	public record struct VariantNumber(int Number)
	{
		internal int ValidNumber => Number < 0 ? 0 : Number > MaxValue ? MaxValue : Number;
		private static VariantNumber Validated(int number) => new VariantNumber(Math.Min(959, Math.Max(0, number)));

		public static implicit operator int (VariantNumber number) => number.ValidNumber;
		public static implicit operator VariantNumber(int number) => VariantNumber.Validated(number);

		internal const int MaxValue = 959;
	}

	public static class Chess960
	{
		public const int UniqueGameCount = 960;

		public static IChessBoard BoardFor(VariantNumber number)
		{
			// https://en.wikipedia.org/wiki/Chess960_numbering_scheme#Direct_derivation

			PieceDef[] pieces = new PieceDef[8];
			Array.Fill(pieces, PieceDef.Default);
			IEnumerable<int> freeIndices()
			{
				for (int i = 0; i < pieces.Length; i++)
					if (pieces[i].IsDefault) yield return i;
			}
			int nthFree(int nth) => freeIndices().Skip(nth).First();

			int n2 = number / 4, b1 = number % 4;
			File f = b1 switch
			{
				0 => Hardware.File.B,
				1 => File.D,
				2 => File.F,
				_ => File.H
			};
			pieces[(int)f] = new PieceDef(PieceType.Bishop, Hue.Light);
			int n3 = n2 / 4, b2 = n2 % 4;
			f = b2 switch
			{
				0 => File.A,
				1 => File.C,
				2 => File.E,
				_ => File.G
			};
			pieces[(int)f] = new PieceDef(PieceType.Bishop, Hue.Light);
			int n4 = n3 / 6, q = n3 % 6;
			int qPos = nthFree(q);
			pieces[qPos] = new PieceDef(PieceType.Queen, Hue.Light);
			int kp1 = 0, kp2 = 0;
			switch (n4)
			{
				case 0: kp2 = 1; break;
				case 1: kp2 = 2; break;
				case 2: kp2 = 3; break;
				case 3: kp2 = 4; break;
				case 4: kp1 = 1; kp2 = 2; break;
				case 5: kp1 = 1; kp2 = 3; break;
				case 6: kp1 = 1; kp2 = 4; break;
				case 7: kp1 = 2; kp2 = 3; break;
				case 8: kp1 = 2; kp2 = 4; break;
				case 9: kp1 = 3; kp2 = 4; break;
			}
			pieces[nthFree(kp1)] = new PieceDef(PieceType.Knight, Hue.Light);
			pieces[nthFree(--kp2)] = new PieceDef(PieceType.Knight, Hue.Light); // decrement because by setting kp1, # free slots is reduced by 1.
			int nFound = 0;
			for (int p = 0; p < pieces.Length; ++p)
			{
				if (pieces[p].IsDefault)
				{
					switch (nFound++)
					{
						case 0: pieces[p] = new PieceDef(PieceType.Rook, Hue.Light); break;
						case 1: pieces[p] = new PieceDef(PieceType.King, Hue.Light); break;
						case 2: pieces[p] = new PieceDef(PieceType.Rook, Hue.Light); break;
					}
				}
			}
			BoardBuilder bb = new BoardBuilder();
			bb.SetPieces(Rank.R1, pieces);
			for (int p = 0; p < pieces.Length; ++p) pieces[p] = pieces[p] with { Hue = Hue.Dark };
			bb.SetPieces(Rank.R8, pieces);
			Array.Fill(pieces, new PieceDef(PieceType.Pawn, Hue.Light));
			bb.SetPieces(Rank.R2, pieces);
			Array.Fill(pieces, new PieceDef(PieceType.Pawn, Hue.Dark));
			bb.SetPieces(Rank.R7, pieces);
			return bb.CreateBoard();
		}

		public static IInteractiveChessGame GameFor(GameSetup gameDefinition, int number)
		{
			gameDefinition = gameDefinition with { Board = new GameBoard(GameBoardType.Custom, BoardFor(number), Hue.Light) };
			return new InteractiveGame(gameDefinition);
		}		
	}
}
