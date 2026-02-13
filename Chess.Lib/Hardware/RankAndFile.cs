using System.Diagnostics;

namespace Chess.Lib.Hardware
{
	public enum Hue { Dark, Light, Default };

	public enum Rank { R1, R2, R3, R4, R5, R6, R7, R8, Offboard };

	public enum File { A, B, C, D, E, F, G, H, Offboard };

	[DebuggerDisplay("{ToSquareIndex}:{File}{Rank}")]
	public record struct FileRank(File File, Rank Rank)
	{
		public static readonly FileRank OffBoard = new FileRank(File.Offboard, Rank.Offboard);
		public bool IsOffBoard => File == File.Offboard || Rank == Rank.Offboard;
		public bool IsOnBoard => !IsOffBoard;
		public string ToEngineMove
		{
			get
			{
				if (IsOffBoard) return string.Empty;
				return $"{File.FileChar}{Rank.RankChar}";
			}
		}
		public static FileRank FromSquareIndex(int squareIndex) => Board.PositionOf(squareIndex);

		public static int SquareIndexOf(File file, Rank rank) => Board.IndexOf(file, rank);

		public FileRank(IChessSquare square): this(square.File, square.Rank) { }

		public int ToSquareIndex => Board.IndexOf(File, Rank);

		public override string ToString() => $"{File.FileChar}{Rank.RankChar}";

		public static FileRank Parse(string engineLocation) =>
			engineLocation != null && engineLocation.Length == 2 ? new FileRank(RFExtensions.ParseFile(engineLocation[0]), RFExtensions.ParseRank(engineLocation[1])) :
			OffBoard;

		public override int GetHashCode() => ToSquareIndex;

		public Hue SquareHue
		{
			get
			{
				if (IsOffBoard) return Hue.Default;
				int squareIndex = ToSquareIndex;
				if ((squareIndex / 8) % 2 == 0)
				{
					return squareIndex % 2 == 0 ? Hue.Dark : Hue.Light;
				}
				return squareIndex % 2 == 0 ? Hue.Light : Hue.Dark;
			}
		}

		public static IEnumerable<FileRank> All
		{
			get
			{
				for(int f = 0;f < 8;++f)
				{
					for(int r = 0; r < 8; ++r)
					{
						File ff = (File)f;
						Rank rr = (Rank)r;
						yield return new FileRank(ff, rr);
					}
				}
			}
		}
	}

	public static class RFExtensions
	{
		extension(Hue h)
		{
			public Hue Other
			{
				get
				{
					switch(h)
					{
						case Hue.Light: return Hue.Dark;
						case Hue.Dark: return Hue.Light;
						default: return Hue.Default;
					}
				}
			}
		}
		extension(File f)
		{
			public char FileChar => (char)('a' + f);
		}

		public static File ParseFile(char loc)
		{
			int nFile = Char.ToLower(loc) - 'a';
			return nFile < 0 || nFile > (int)File.H ? File.Offboard : (File)nFile;
		}

		extension(Rank r)
		{
			public char RankChar => (char)('1' + r);

		}
		public static Rank ParseRank(char loc)
		{
			if (int.TryParse(loc.ToString(), out int nRank) && nRank > 0 && (nRank - 1) <= (int)Rank.R8) return (Rank)(nRank - 1);
			return Rank.Offboard;
		}
	}
}
