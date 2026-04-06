using Chess.Lib.Pgn.DataModel;
using System.Collections;

namespace Chess.Lib.Pgn.Service
{
	public interface IDynamicReadOnlyList<T> : IReadOnlyList<T>
	{
		long TotalCount { get; }
		long CurrentCount { get; }

		bool IsComplete => CurrentCount == TotalCount;
	}

	internal class GameIterator : IDynamicReadOnlyList<PgnGame>
	{
		internal GameIterator(): this(PgnGameService.LoadGames(1)) { }
		internal GameIterator(List<PgnGame> games)
		{
			TotalCount = PgnGameService.TotalGameCount();
			Games = games;
		}

		public long TotalCount { get; private init; }

		public long CurrentCount { get; set; }

		PgnGame IReadOnlyList<PgnGame>.this[int index] => Games[index];

		internal List<PgnGame> Games { get; private set; }
		int IReadOnlyCollection<PgnGame>.Count => Games.Count;

		IEnumerator<PgnGame> IEnumerable<PgnGame>.GetEnumerator() => new PgnGameEnumerator(this, Games);

		IEnumerator IEnumerable.GetEnumerator() => Games.GetEnumerator();

		private void SetGames(List<PgnGame> batch)
		{
			CurrentCount += batch.Count;
			Games = batch;
		}

		private class PgnGameEnumerator : IEnumerator<PgnGame>
		{
			internal PgnGameEnumerator(GameIterator owner, List<PgnGame> games)
			{
				Owner = owner;
				Current = PgnGame.Empty;
			}

			private GameIterator Owner { get; init; }
			private List<PgnGame> Games => Owner.Games;
			private int Index { get; set; } = -1;
			public PgnGame Current { get; private set; }
			object IEnumerator.Current => Current;

			public void Dispose() { }

			public bool MoveNext()
			{
				if(Index < Games.Count - 1)
				{
					Current = Games[++Index];
					return true;
				} else
				{
					if (Games.Count == 0) return false;
					Owner.Games = PgnGameService.LoadGames(Games.Last().Id + 1);
					if (Games.Count == 0) return false;
					Index = 0;
					Current = Games[Index];
					return true;
				}
			}

			public void Reset()
			{
				Index = -1;
			}
		}
	}
}
