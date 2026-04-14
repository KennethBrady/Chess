using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn.DataModel;
using Chess.Lib.Pgn.Parsing;
using Chess.Lib.Pgn.Service;
using Chess.Lib.UI.Pgn;
using Common.Lib.Extensions;
using Common.Lib.UI.Controls.Models;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.DragDrop;
using Common.Lib.UI.MVVM;
using System.Collections.Immutable;
using System.IO;
using System.Net.WebSockets;
using System.Windows.Input;
using System.Windows.Media;

namespace PgnImporter.Models
{
	public partial class MainModel : MainWindowModel
	{
		internal static new MainModel Instance => (MainModel)MainWindowModel.Instance;

		private List<GameImportModel> _imports = new();
		private GameImportModel? _selection;
		private const string BuilderFileName = "builders.bin";
		private RelayCommand<GameImportModel> _showPgn;
		public MainModel(IPgnImporterWindow window) : base(window)
		{
			ChessDB.Initialize();
			DropHandler = new FileDDStrategy(HandleDrop);
			Status = new();
			GameSource latest = ChessDB.GameSources.Latest;
			if (latest.IsEmpty) Status.AddEntry("No game sources have been added.");
			else
				Status.AddEntry($"Last imported Game Source: {latest.Name} ({latest.Id})");
			Progress = new StackedValuesModel(Brushes.Lime, Brushes.Red);
			if (File.Exists(BuilderFileName))
			{
				List<GameImport> imps;
				(SourcePath, imps) = Extensions.Read(BuilderFileName);
				_imports = imps.Select(b => new GameImportModel(b)).ToList();
				Progress.Maximum = _imports.Count;
			}
			ImportStatus = new ImportStatusModel();
			_showPgn = new RelayCommand<GameImportModel>(ExecutePgn);
		}

		public StatusBarModel Status { get; private init; }

		public StackedValuesModel Progress { get; private init; }

		public IDDStrategy DropHandler { get; private init; }

		public IEnumerable<GameImportModel> Imports => _imports;

		public GameImportModel? Selection
		{
			get => _selection;
			set
			{
				_selection = value;
				Notify(nameof(Selection));
			}
		}

		public string ActionLabel
		{
			get
			{
				switch (ImportStep)
				{
					case PgnImportStatus.None: return "Match Players";
					case PgnImportStatus.PlayersLocated: return "Parse Moves";
					case PgnImportStatus.PgnMovesParsed: return "Verify Uniqueness";
					case PgnImportStatus.UniquenessVerified: return "Match Openings";
					default: return "Import into DB";
				}
			}
		}

		public string SourcePath { get; private set; } = string.Empty;

		public string SourcePathAndCount => _imports.Count == 0 ? SourcePath : $"{SourcePath} ({_imports.Count:N0})";

		public ImportStatusModel ImportStatus { get; private init; }

		public new IPgnImporterWindow Window => (IPgnImporterWindow)base.Window;

		public ICommand ShowPgnCommand => _showPgn;

		private PgnImportStatus ImportStep { get; set; } = PgnImportStatus.None;

		protected override bool CanExecute(string? parameter)
		{
			switch(parameter)
			{
				case "action": return !IsBusy && _imports.Count > 0;
				case "twic": return _imports.Count == 0;
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch(parameter)
			{
				case "twic": DownloadFromTWIC(); break;
				case "action":
					switch (ImportStep)
					{
						case PgnImportStatus.None: MatchPlayers(); break;
						case PgnImportStatus.PlayersLocated: ParseAllMoves(); break;
						case PgnImportStatus.PgnMovesParsed: VerifyGamesUnique(); break;
						case PgnImportStatus.UniquenessVerified: MatchOpenings(); break;
						case PgnImportStatus.OpeningsMatched: Import(); break;
					}
					break;
			}
		}

		private async void ExecutePgn(GameImportModel? model)
		{
			if (model == null) return;
			PgnEditorModel m = new PgnEditorModel(model.Builder);
			await ShowDialog(m);
		}

		private async void HandleDrop(IEnumerable<string> files)
		{
			string? fpath = files.FirstOrDefault();
			if (string.IsNullOrEmpty(fpath) || !File.Exists(fpath)) return;
			Window.Activate();
			var result = await ShowDialog(new ImportFileDialogModel(fpath));
			if (result is IDialogResultAccepted<ImportFileResult> acc)
			{
				_imports = acc.Value.Imports.Select(i => new GameImportModel(i)).ToList();
				SourcePath = fpath;
				Notify(nameof(Imports), nameof(SourcePath), nameof(SourcePathAndCount));
				ImportStep = PgnImportStatus.None;
				Progress.Maximum = _imports.Count;
				RaiseCanExecuteChanged();
				Extensions.Save(BuilderFileName, fpath, _imports.Select(i => i.Builder.Import), _imports.Count);
				ImportStatus.Reset(Path.GetFileName(SourcePath));
			}
		}

		private bool IsBusy { get; set; }

		#region Import Step Methods
		private async void MatchPlayers()
		{
			IsBusy = true;
			RaiseCanExecuteChanged();
			Progress.Maximum = 2 * _imports.Count;
			PlayerFeedback? lastFeedback = null;
			void update(PlayerFeedback feedback)
			{
				Progress.SetValues(feedback.NFound, feedback.NNotFound);
				Progress.SetText($"{feedback.NFound:N0} Players matched; {feedback.NNotFound:N0} new players");
				foreach(var m in feedback.Updates)
				{
					m.Import.SetPlayers(m.FoundWhite, m.FoundBlack);
				}
				if(feedback.Updates.Count() > 0) 
				Window.ScrollToGame(feedback.Updates.Last().Import);
				lastFeedback = feedback;
			}
			await MatchPlayers(update);
			ImportStep = PgnImportStatus.PlayersLocated;
			Notify(nameof(ActionLabel));
			IsBusy = false;
			RaiseCanExecuteChanged();
			if (lastFeedback.HasValue)
			{
				ImportStatus.SetResults(PgnImportStatus.PlayersLocated, lastFeedback.Value.NFound, lastFeedback.Value.NNotFound);
			}
		}


		private readonly record struct PlayerImportUpdate(GameImportModel Import, PgnPlayer? FoundWhite, PgnPlayer? FoundBlack);
		private readonly record struct PlayerFeedback(int NFound, int NNotFound, IEnumerable<PlayerImportUpdate> Updates);

		private async Task MatchPlayers(Action<PlayerFeedback> feedback)
		{
			Dictionary<int, PgnPlayer> dPlayers = ChessDB.Players.Values.Where(p => p.FideId > 0).ToDictionary(p => p.FideId);
			List<PgnPlayer> players = ChessDB.Players.Values.ToList();
			players.Sort(PgnPlayer.NameComparer);
			int nTotal = 0, nFound = 0, nNotFound = 0;
			List<PlayerImportUpdate> updates = new();
			foreach (GameImportModel gim in Imports)
			{
				PgnPlayer? wh = null, bl = null;
				if (gim.WhitePlayer.FideId > 0 && dPlayers.ContainsKey(gim.WhitePlayer.FideId)) wh = dPlayers[gim.WhitePlayer.FideId];
				else
				{
					int n = players.BinarySearch(gim.WhitePlayer, PgnPlayer.NameComparer);
					if (n >= 0) wh = players[n];
				}
				if (gim.BlackPlayer.FideId > 0 && dPlayers.ContainsKey(gim.BlackPlayer.FideId)) bl = dPlayers[gim.BlackPlayer.FideId];
				else
				{
					int n = players.BinarySearch(gim.BlackPlayer, PgnPlayer.NameComparer);
					if (n >= 0) bl = players[n];
				}
				if (wh == null) nNotFound++; else nFound++;
				if (bl == null) nNotFound++; else nFound++;
				updates.Add(new PlayerImportUpdate(gim, wh, bl));
				if (++nTotal % 25 == 0)
				{
					PlayerFeedback fb = new PlayerFeedback(nFound, nNotFound, updates);
					feedback(fb);
					updates.Clear();
					await Task.Delay(25);
				}
			}
			if (updates.Count > 0) feedback(new PlayerFeedback(nFound, nNotFound, updates));
		}

		private async void ParseAllMoves()
		{
			IsBusy = true;
			RaiseCanExecuteChanged();
			Progress.ClearAll();
			Progress.Maximum = _imports.Count;
			int nSuccess = 0, nError = 0;
			for (int i = 0; i < _imports.Count; i++)
			{
				GameImportModel gim = _imports[i];
				if (i % 25 == 0)
				{
					Window.ScrollToGame(gim);
					await Task.Delay(100);
				}
				if (gim.ParseMoves()) nSuccess++; else nError++;
				Progress.SetValues(nSuccess, nError);
				Progress.Text = $"{nSuccess:N0} / {_imports.Count:N0} games parsed successfully";
			}
			ImportStep = PgnImportStatus.PgnMovesParsed;
			IsBusy = false;
			RaiseCanExecuteChanged();
			Notify(nameof(ActionLabel));
			ImportStatus.SetResults(PgnImportStatus.PgnMovesParsed, nSuccess, nError);
		}

		private async void VerifyGamesUnique()
		{
			IsBusy = true;
			RaiseCanExecuteChanged();
			Progress.ClearAll();
			Progress.Maximum = _imports.Count;
			var builders = _imports.Select(i => i.Builder);
			IEnumerable<int> wIds = builders.Where(b => b.Import.White.Id > 0).Select(b => b.Import.White.Id).Distinct(),
				bIds = builders.Where(b => b.Import.Black.Id > 0).Select(b => b.Import.Black.Id).Distinct();
			List<PgnGame> games = await ChessDB.Games.GamesWithPlayers(wIds, bIds);
			PlayerGameComparer cmp = new PlayerGameComparer();
			games.Sort(cmp);
			int nUnique = 0, nDupe = 0;
			List<int> dupeIds = new();
			for (int i = 0; i < _imports.Count; i++)
			{
				dupeIds.Clear();
				GameImportModel gim = _imports[i];
				PgnGameBuilder b = gim.Builder;
				if (b.Import.White.Id > 0 && b.Import.Black.Id > 0)
				{
					PgnGame tmp = PgnGame.Empty with { WhiteId = b.Import.White.Id, BlackId = b.Import.Black.Id };
					IEnumerable<int> ndxs = games.BinarySearchAll(tmp, cmp);
					foreach(int ndx in ndxs)
					{
						if (string.Equals(b.Moves, games[ndx].Moves)) dupeIds.Add(games[ndx].Id);						
					}
				}
				gim.SetUnique(dupeIds);
				if (dupeIds.Count == 0) nUnique++; else nDupe++;
				Progress.SetValues(nUnique, nDupe);
				Progress.Text = $"{nUnique:N0} / {_imports.Count:N0} Unique Games";
				if (i % 25 == 0)
				{
					Window.ScrollToGame(gim);
					await Task.Delay(10);
				}
			}
			IsBusy = false;
			ImportStep = PgnImportStatus.UniquenessVerified;
			Notify(nameof(ActionLabel));
			RaiseCanExecuteChanged();
			ImportStatus.SetResults(PgnImportStatus.UniquenessVerified, nUnique, nDupe);
		}

		private async void MatchOpenings()
		{
			IsBusy = true;
			RaiseCanExecuteChanged();
			Progress.ClearAll();
			Progress.Maximum = _imports.Count;
			List<Opening> openings = ChessDB.Openings.Values.ToList();
			openings.Sort((ox, oy) => -ox.Sequence.Length.CompareTo(oy.Sequence.Length));
			int nMatched = 0, nNotMatched = 0;
			for (int i = 0; i < _imports.Count; i++)
			{
				GameImportModel gim = _imports[i];
				Opening o = openings.First(oo => gim.Builder.Moves.StartsWith(oo.Sequence));
				gim.SetOpening(o);
				if (o.Id > 1) nMatched++;else nNotMatched++;
				if (i % 25 == 0)
				{
					Window.ScrollToGame(gim);
					Progress.SetValues(nMatched, nNotMatched);
					Progress.SetText($"{nMatched:N0} / {_imports.Count:N0} openings matched");
					await Task.Delay(10);
				}
			}
			Progress.SetValues(nMatched, nNotMatched);
			Progress.SetText($"{nMatched:N0} / {_imports.Count:N0} openings matched");
			IsBusy = false;
			RaiseCanExecuteChanged();
			ImportStep = PgnImportStatus.OpeningsMatched;
			Notify(nameof(ActionLabel));
			ImportStatus.SetResults(PgnImportStatus.OpeningsMatched, nMatched, nNotMatched);
		}

		private async void Import()
		{
			Progress.Maximum = _imports.Count;
			try
			{
				IsBusy = true;
				List<PgnGame> inserted = await ChessDB.Games.Insert(Path.GetFileName(SourcePath), _imports.Select(im => im.Builder), ins =>
				{
					Progress.SetValues(ins.Index, 0);
					Progress.SetText($"{(1 + ins.Index):N0} games inserted.");
					if (ins.Index % 25 == 0) Window.Dispatch(() => Window.ScrollToGame(_imports[ins.Index]));
				});
				ImportStatus.SetResults(PgnImportStatus.Completed, _imports.Count, 0);
				if (File.Exists(BuilderFileName)) File.Delete(BuilderFileName);
				_imports = new();
				ImportStep = PgnImportStatus.None;
				SourcePath = string.Empty;
				Notify(nameof(Imports), nameof(ImportStep), nameof(SourcePath), nameof(SourcePathAndCount), nameof(ActionLabel));
				var latest = ChessDB.GameSources.Latest;
				Status.AddEntry($"Last imported Game Source: {latest.Name} ({latest.Id})");
			}
			catch(Exception ex)
			{
				Status.AddEntry(ex);
			}
			finally
			{
				IsBusy = false;
				RaiseCanExecuteChanged();
			}
		}

		#endregion

		public record ImportResult(string Operation, string Result);

		private void DownloadFromTWIC()
		{
			ShowDialog(new TWICDownloadDialogModel());
		}

		private class PlayerGameComparer : IComparer<PgnGame>
		{
			public int Compare(PgnGame? x, PgnGame? y)
			{
				if (x == null) return y == null ? 0 : 1;
				if (y == null) return -1;
				int ix = x.WhiteId, iy = y.WhiteId;
				switch(Comparer<int>.Default.Compare(ix, iy))
				{
					case < 0: return -1;
					case > 0: return 1;
					default:
						ix =x.BlackId; iy = y.BlackId;
						return Comparer<int>.Default.Compare(ix, iy);
				}
			}
		}
	}

	#region Extensions
	internal static class Extensions
	{
		public static void Save(string destPath, string sourcePath, List<GameImport> imports)
		{
			using var f = File.OpenWrite(destPath);
			using var w = new BinaryWriter(f);
			w.Write(sourcePath);
			w.Write(imports.Count);
			foreach (var i in imports) i.Serialize(w);
		}
		public static void Save(string fpath, string sourcePath, IEnumerable<GameImport> imports, int count)
		{
			using var f = File.OpenWrite(fpath);
			using var w = new BinaryWriter(f);
			w.Write(sourcePath);
			w.Write(count);
			foreach (var b in imports) b.Serialize(w);
		}

		public static (string, List<GameImport>) Read(string fpath)
		{
			using var f = File.OpenRead(fpath);
			using var r = new BinaryReader(f);
			string sourcePath = r.ReadString();
			int n = r.ReadInt32();
			List<GameImport> bldrs = new List<GameImport>(n);
			for (int i = 0; i < n; i++)
			{
				bldrs.Add(ReadImport(r));
			}
			return (sourcePath, bldrs);
		}

		private static PgnGameBuilder ReadBuilder(BinaryReader rdr)
		{
			GameImport imp = ReadImport(rdr);
			return new PgnGameBuilder(imp, rdr.ReadInt32(), ImmutableList<int>.Empty, (GameResult)rdr.ReadInt32());
		}

		extension(PgnGameBuilder builder)
		{
			public PgnPlayer White => builder.Import.White;
			public PgnPlayer Black => builder.Import.Black;
			public string Moves => builder.Import.Moves;
			public int WhiteId => builder.Import.White.Id;
			public int BlackId => builder.Import.Black.Id;
		}

		private static GameImport ReadImport(BinaryReader rdr)
		{
			string siName = rdr.ReadString();
			int pos = rdr.ReadInt32(), ndx = rdr.ReadInt32();
			string pgn = rdr.ReadString();
			DateTime evtDate = new DateTime(rdr.ReadInt64());
			string wName = rdr.ReadString();
			int wId = rdr.ReadInt32();
			string bName = rdr.ReadString();
			int bId = rdr.ReadInt32();
			string moves = rdr.ReadString();
			GameResult gr = (GameResult)rdr.ReadInt32();
			bool hasUnex = rdr.ReadBoolean();
			int nTags = rdr.ReadInt32();
			var bldr = ImmutableDictionary.CreateBuilder<string, string>();

			for (int i = 0; i < nTags; ++i) bldr.Add(rdr.ReadString(), rdr.ReadString());
			return new GameImport(
				new SourceInfo(siName, pos, ndx), pgn, evtDate,
				new PgnPlayer(0, wName, wId),
				new PgnPlayer(0, bName, bId), moves, gr, hasUnex, bldr.ToImmutableDictionary());

		}

		extension(GameImport imp)
		{
			public void Serialize(BinaryWriter w)
			{
				w.Write(imp.SourceInfo.Name);
				w.Write(imp.SourceInfo.Position);
				w.Write(imp.SourceInfo.Index);
				w.Write(imp.Pgn);
				w.Write(imp.EventDate.Ticks);
				w.Write(imp.White.Name);
				w.Write(imp.White.FideId);
				w.Write(imp.Black.Name);
				w.Write(imp.Black.FideId);
				w.Write(imp.Moves);
				w.Write((int)imp.Result);
				w.Write(imp.HasUnexpectedMoves);
				w.Write(imp.Tags.Count);
				foreach (var t in imp.Tags)
				{
					w.Write(t.Key);
					w.Write(t.Value);
				}
			}
		}

	}

	#endregion
}
