namespace Chess.Lib.Hardware
{
	public enum Hue { Dark, Light, Default };

	public enum Rank { R1, R2, R3, R4, R5, R6, R7, R8, Offboard };

	public enum File { A, B, C, D, E, F, G, H, Offboard };

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
				return $"{File.FileChar()}{Rank.RankChar()}";
			}
		}
		public static FileRank FromSquareIndex(int squareIndex) => Board.PositionOf(squareIndex);

		public static int SquareIndexOf(File file, Rank rank) => Board.IndexOf(file, rank);

		public FileRank(IChessSquare square): this(square.File, square.Rank) { }

		public int ToSquareIndex => Board.IndexOf(File, Rank);

		public override string ToString() => $"{File.FileChar()}{Rank.RankChar()}";

		public static FileRank Parse(string engineLocation) =>
			engineLocation != null && engineLocation.Length == 2 ? new FileRank(FileEx.Parse(engineLocation[0]), RankEx.Parse(engineLocation[1])) :
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
	}

	public static class HueEx
	{
		public static Hue Other(this Hue h) => h == Hue.Light ? Hue.Dark : h == Hue.Dark ? Hue.Light : Hue.Dark;
	}

	internal static class FileEx
	{
		public static char FileChar(this File f) => (char)('a' + f);
		public static File Parse(char loc)
		{
			int nFile = loc - 'a';
			return nFile < 0 || nFile > (int)File.H ? File.Offboard : (File)nFile;
		}
	}

	internal static class RankEx
	{
		public static char RankChar(this Rank r) => (char)('1' + r);

		public static Rank Parse(char loc)
		{
			if (int.TryParse(loc.ToString(), out int nRank) && nRank > 0 && (nRank -1) <= (int)Rank.R8) return (Rank)(nRank - 1);
			return Rank.Offboard;
		}
	}
}
