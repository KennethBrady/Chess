using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Timing;
using System.Xml.Linq;

namespace Chess.Lib.Games
{
	public enum GameBoardType { Classic, FischerRandom, Custom };

	public record struct GameBoard(GameBoardType Type, IChessBoard Board, Hue NextMove)
	{
		public static readonly GameBoard Default = new GameBoard(GameBoardType.Classic, NoBoard.Instance, Hue.Default);

		internal IBoard IBoard => (IBoard)Board;

		internal XElement ToXml()
		{
			XElement r = new XElement(nameof(GameBoard));
			r.Add(new XAttribute(nameof(Type), Type));
			r.Add(new XAttribute(nameof(NextMove), NextMove));
			r.Add(new XAttribute(nameof(Board), Board.FENPiecePlacements));
			return r;
		}

		internal static GameBoard FromXml(XElement e)
		{
			GameBoard r = Default;
			foreach (XAttribute x in e.Attributes())
			{
				switch (x.Name.LocalName)
				{
					case nameof(Type): r = r with { Type = Enum.Parse<GameBoardType>(x.Value) }; break;
					case nameof(NextMove): r = r with { NextMove = Enum.Parse<Hue>(x.Value) }; break;
					case nameof(Board): r = r with { Board = FEN.Parse(x.Value).ToBoard() }; break;
				}
				;
				break;
			}
			return r;
		}
	}

	public record struct GameSetup(string WhiteName, string BlackName, ChessClockSetup ClockSetup, GameBoard Board)
	{
		public static readonly GameSetup Default = new GameSetup(string.Empty, string.Empty, ChessClockSetup.Empty, GameBoard.Default);

		public static GameSetup Named(string whiteName, string blackName) => Default with { WhiteName = whiteName, BlackName = blackName };

		public static GameSetup FromXml(string sXml)
		{
			string wName = string.Empty, bName = string.Empty;
			ChessClockSetup css = ChessClockSetup.Empty;
			GameBoard gb = GameBoard.Default;
			XElement? x = null;
			try
			{
				x = XElement.Parse(sXml);
			}
			catch
			{
				return Default;
			}
			foreach (XAttribute a in x.Attributes())
			{
				switch (a.Name.LocalName)
				{
					case nameof(WhiteName): wName = a.Value; break;
					case nameof(BlackName): bName = a.Value; break;
				}
			}
			foreach (XElement e in x.Elements())
			{
				switch (e.Name.LocalName)
				{
					case nameof(ChessClockSetup): css = ChessClockSetup.FromXml(e); break;
					case nameof(GameBoard): gb = GameBoard.FromXml(e); break;
				}
			}
			return new GameSetup(wName, bName, css, gb);
		}
	}

	public static class GameSetupExtensions
	{
		extension(GameSetup gameSetup)
		{
			public string ToXml()
			{
				XElement gs = new XElement(nameof(GameSetup));
				gs.Add(new XAttribute(nameof(GameSetup.WhiteName), gameSetup.WhiteName));
				gs.Add(new XAttribute(nameof(GameSetup.BlackName), gameSetup.BlackName));
				gs.Add(gameSetup.ClockSetup.ToXml());
				gs.Add(gameSetup.Board.ToXml());
				return gs.ToString();
			}
		}
	}
}
