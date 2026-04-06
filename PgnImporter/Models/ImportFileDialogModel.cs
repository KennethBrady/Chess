using Chess.Lib.Pgn.Parsing;
using Common.Lib.IO;
using Common.Lib.UI.Controls.Models;
using Common.Lib.UI.Dialogs;
using CommonApps.Lib;
using System.IO;
using System.Windows.Media;

namespace PgnImporter.Models
{
	public readonly record struct ImportFileResult(bool Accepted, List<GameImport> Imports);
	public class ImportFileDialogModel : DialogModel<ImportFileResult>
	{
		internal ImportFileDialogModel(string filePath)
		{
			FilePath = filePath;
			Progress = new StackedValuesModel(Brushes.Lime, Brushes.Red);
			string fname = Path.GetFileNameWithoutExtension(filePath), ext = Path.GetExtension(filePath);
			if (SevenZip.IsArchiveFile(FilePath)) Unzip(); else PgnFilePath = FilePath;
		}

		public string FilePath { get; private init; }

		public StackedValuesModel Progress { get; private init; }

		public string ErrorMessage { get; private set; } = string.Empty;

		public bool IsParsing { get; private set; }

		public string ButtonLabel => IsParsed ? "Apply Games" : "Parse Pgn";
		private bool IsParsed => Imports != null && Imports.Count > 0;

		private List<GameImport>? Imports { get; set; }
		protected override bool CanExecute(string? parameter)
		{
			switch(parameter)
			{
				case CancelParameter: return true;
				case "parsePgn": return !IsParsing;
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch(parameter)
			{
				case CancelParameter: Cancel(); break;
				case "parsePgn":
					if (Imports != null) Accept(new ImportFileResult(true, Imports)); else ParsePgn();
					break;
			}
		}

		private string PgnFilePath { get; set; } = string.Empty;

		private TempFolder? TempFolder { get; set; }
		private async void Unzip()
		{
			TempFolder = new TempFolder();
			var result = await SevenZip.Extract(FilePath, TempFolder);
			if (result.Succeeded)
			{
				PgnFilePath = Directory.EnumerateFiles(TempFolder.FolderPath).First();
			} else
			{
				ErrorMessage = result.ErrorString;
				Notify(nameof(ErrorMessage));
			}
		}

		private async void ParsePgn()
		{
			Progress.Text = "Getting PGN Game Count ...";
			IsParsing = true;
			RaiseCanExecuteChanged();
			var imports = await PgnSourceParser.ParseFromFileAsync(PgnFilePath, progress =>
			{
				Progress.Maximum = progress.TotalParsed;
				Progress.SetValues(progress.NSuccess, progress.NFail);
				Progress.Text = $"{progress.PercentComplete:F2} % Complete";
			});
			Imports = imports.Where(i => i.Succeeded).Cast<IPgnParseSuccess>().Select(i => i.Import).ToList();
			IsParsing = false;
			RaiseCanExecuteChanged();
			Notify(nameof(ButtonLabel));
		}

		public override void Dispose()
		{
			TempFolder?.Dispose();
		}

	}
}
