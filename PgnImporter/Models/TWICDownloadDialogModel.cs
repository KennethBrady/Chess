using Chess.Lib.Pgn.Service;
using Common.Lib.Contracts;
using Common.Lib.Extensions;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.Extensions;
using Common.Lib.UI.MVVM;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ZstdSharp.Unsafe;

namespace PgnImporter.Models
{
	public readonly record struct TWICDownloadResult(ImmutableList<string> DownloadedFiles);
	public class TWICDownloadDialogModel : DialogModel<TWICDownloadResult>
	{
		private const string TWICUrl = @"https://theweekinchess.com/twic";
		private static readonly HttpClient _client = HttpClientExtensions.Create();
		private static readonly Regex _rxTable = new Regex(@"<table class=\""results-table\"">");
		private static readonly Regex _rxUrl = new Regex(@"<a href=\""(.*)\""");
		private static readonly Regex _rxTD = new Regex(@"<td>(.*)</td>");

		private List<TWICEntryModel> _entries = new();
		private ICollectionView _view;
		private bool _showExisting;
		private string _folder = string.Empty;
		private List<string> _downloads = new();
		public TWICDownloadDialogModel()
		{
			_view = _entries.GetDefaultView;
			_view.Filter = ShowEntry;
			FindTWICEntries();
			_folder = Settings.Default.DownloadFolder;
		}

		public object Entries => _view;

		public string DownloadLabel => IsDownloadComplete ? "Close" : "Download";

		public string Folder
		{
			get => _folder;
			set
			{
				_folder = value;
			}
		}

		public bool ShowExisting
		{
			get => _showExisting;
			set
			{
				_showExisting = value;
				Notify(nameof(ShowExisting));
				_view.Refresh();
			}
		}

		public int DownloadCount { get; private set; }
		public int CurrentDownload { get; private set; }

		public string CurrentFile { get; private set; } = string.Empty;

		public Action<object> ScrollGrid { get; set; } = Actions<object>.Empty;

		public bool IsReady { get; private set; }

		public bool IsDownloading { get; private set; }
		public bool IsDownloadComplete { get; private set; }

		public Func<string, Task<string>>? BrowseFolder { get; set; }

		protected override bool CanExecute(string? parameter)
		{
			if (!IsReady || IsDownloading) return false;
			switch (parameter)
			{
				case CancelParameter:
				case "browse": return true;
				case OKParameter: return !IsDownloading && !string.IsNullOrEmpty(_folder) && Directory.Exists(_folder);
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case CancelParameter: Cancel(); break;
				case OKParameter:
					if (IsDownloadComplete)
						Accept(new TWICDownloadResult(ImmutableList<string>.Empty.AddRange(_entries.Where(e => !e.IsInDatabase).Select(e => e.DownloadPath))));
					else Download();
					break;
				case "browse": BrowseForFolder(); break;
			}
		}

		private async void FindTWICEntries()
		{
			const string ENDTABLE = "</table>";
			string html = await _client.GetStringAsync(TWICUrl);
			Match mTable = _rxTable.Match(html);
			if (mTable.Success)
			{
				int endTable = html.IndexOf(ENDTABLE, mTable.Index);
				string table = html.Substring(mTable.Index, endTable - mTable.Index + ENDTABLE.Length);
				MatchCollection tds = _rxTD.Matches(table);
				int nTd = 0;
				while (nTd + 6 < tds.Count)
				{
					string sNum = tds[nTd].Groups[1].Value, sDate = tds[nTd + 1].Groups[1].Value, sUrl = tds[nTd + 3].Groups[1].Value, sCount = tds[nTd + 5].Groups[1].Value;
					Match mUrl = _rxUrl.Match(sUrl);
					if (!mUrl.Success) break;
					sUrl = mUrl.Groups[1].Value;
					if (int.TryParse(sNum, out int num) && int.TryParse(sCount, out int count)) _entries.Add(new TWICEntryModel(num, sDate, sUrl, count));
					nTd += 7;
				}
			}
			_view.Refresh();
			IsReady = true;
			RaiseCanExecuteChanged();
			if (Directory.Exists(_folder)) _entries.ForEach(e => e.CheckFileExists(_folder));
		}

		private bool ShowEntry(object oEntry)
		{
			if (oEntry is not TWICEntryModel m) return false;
			return _showExisting || !m.IsInDatabase;
		}

		private async void BrowseForFolder()
		{
			if (BrowseFolder == null) return;
			string folder = await BrowseFolder(_folder);
			if (!string.IsNullOrEmpty(folder))
			{
				_folder = folder;
				Notify(nameof(Folder));
				_entries.ForEach(e => e.CheckFileExists(_folder));
			}
		}

		private async void Download()
		{
			Settings.Default.DownloadFolder = _folder;
			IsDownloading = true;
			DownloadCount = _entries.Where(e => !e.IsInDatabase && !e.FileExists).Count();
			Notify(nameof(IsDownloading), nameof(DownloadCount));
			foreach (TWICEntryModel m in _entries)
			{
				if (m.IsInDatabase) continue;
				if (m.FileExists) continue;
				ScrollGrid(m);
				CurrentFile = m.FileName;
				Notify(nameof(CurrentFile));
				var bytes = await _client.GetByteArrayAsync(m.Url);
				string fpath = Path.Combine(_folder, m.FileName);
				File.WriteAllBytes(fpath, bytes);
				m.DownloadPath = fpath;
				CurrentDownload++;
				Notify(nameof(CurrentDownload));
			}
			IsDownloading = false;
			IsDownloadComplete = true;
			Notify(nameof(IsDownloadComplete), nameof(DownloadLabel));
			RaiseCanExecuteChanged();
		}

		public class TWICEntryModel : ViewModel
		{
			internal TWICEntryModel(int id, string sDate, string url, int nGames)
			{
				Id = id;
				Date = sDate;
				NGames = nGames;
				Url = url;
				IsInDatabase = ChessDB.GameSources.Contains(FileName);
			}

			public int Id { get; private init; }

			public string Date { get; private init; }

			public int NGames { get; private init; }
			public string Url { get; private init; }

			public bool IsInDatabase { get; private init; }

			public bool FileExists { get; private set; }

			internal void CheckFileExists(string fpath)
			{
				string fullpath = Path.Combine(fpath, FileName);
				FileExists = File.Exists(fullpath);
				Notify(nameof(FileExists));
			}

			internal string DownloadPath { get; set; } = string.Empty;
			internal string FileName => $"twic{Id}g.zip";
		}
	}
}
