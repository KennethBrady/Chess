using SpreadsheetLight;
using Sql.Lib.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sql.Lib.Scoring.IO
{
	public static class ScoringFileUpdater
	{
		internal const int RefQueryColumnIndex = 7;
		internal const int StudentQueryColumnIndex = 2;

		internal static string WorksheetNameForSqlType(SqlType type)
		{
			return (type == SqlType.DDL) ? "Schemas" : "Queries";
		}

		private static void UpdateScripts(string scriptsPath, SqlType type, string scoreFilePath, int columnIndex)
		{
			if ((type != SqlType.DDL) && (type != SqlType.DQL)) throw new ArgumentException(nameof(type));
			SqlParser scripts = new SqlParser(File.ReadAllText(scriptsPath), type);
			UpdateScripts(scripts, scoreFilePath, columnIndex);
		}

		private static void UpdateScripts(IEnumerable<ParseResult> scripts, string scoreFilePath, int columnIndex)
		{
			if (scripts.Count() == 0) return;
			SqlType type = scripts.First().Type;
			if (!scripts.All(s => s.Type == type)) throw new ArgumentException("All SQL scripts must be of the same type.");
			using (SLDocument document = new SLDocument(scoreFilePath))
			{
				string wsName = WorksheetNameForSqlType(type);
				if (!document.SelectWorksheet(wsName)) throw new ArgumentException($"Worksheet '{wsName}' not found in file {scoreFilePath}.");
				int nRow = 2;
				foreach (ParseResult pr in scripts.Where(s => s.Type == type))
				{
					document.SetCellValue(nRow++, columnIndex, pr.FullScript);
				}
				var style = document.CreateStyle();
				style.SetWrapText(true);
				style.SetVerticalAlignment(DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center);
				document.SetColumnStyle(columnIndex, style);
				document.SelectWorksheet(document.GetWorksheetNames().First());
				document.Save();
			}
		}

		public static void UpdateReferenceScripts(string referenceScriptsPath, SqlType type, string scoreFilePath)
		{
			UpdateScripts(referenceScriptsPath, type, scoreFilePath, RefQueryColumnIndex);
		}

		public static void UpdateReferenceScripts(IEnumerable<ParseResult> scripts, string scoreFilePath)
		{
			UpdateScripts(scripts, scoreFilePath, RefQueryColumnIndex);
		}

		public static int UpdateReferenceScripts(string referenceScriptsPath, SqlType type, IEnumerable<string> scoreFilePaths)
		{
			int r = 0;
			foreach (string s in scoreFilePaths)
			{
				UpdateReferenceScripts(referenceScriptsPath, type, s);
				r++;
			}
			return r;
		}

		public static Task<int> UpdateReferenceScriptsAsync(string referenceScriptsPath, SqlType type, IEnumerable<string> scoreFilePaths)
		{
			return Task<int>.Factory.StartNew(() => UpdateReferenceScripts(referenceScriptsPath, type, scoreFilePaths));
		}

		public static int UpdateReferenceScripts(IEnumerable<ParseResult> referenceScripts, IEnumerable<string> scoreFilePaths)
		{
			int r = 0;
			foreach(string fpath in scoreFilePaths)
			{
				UpdateReferenceScripts(referenceScripts, fpath);
				r++;
			}
			return r;
		}

		public static Task<int> UpdateReferenceScriptsAsync(IEnumerable<ParseResult> referenceScripts, IEnumerable<string> scoreFilePaths)
		{
			return Task<int>.Factory.StartNew(() => UpdateReferenceScripts(referenceScripts, scoreFilePaths));
		}

		public static void UpdateStudentScripts(string studentScriptsPath, SqlType type, string scoreFilePath)
		{
			UpdateScripts(studentScriptsPath, type, scoreFilePath, StudentQueryColumnIndex);
		}

		public static void UpdateStudentScripts(IEnumerable<ParseResult> studentScripts, string scoreFilePath)
		{
			UpdateScripts(studentScripts, scoreFilePath, StudentQueryColumnIndex);
		}

		public static Task UpdateStudentScriptsAsync(string studentScriptsPath, SqlType type, string scoreFilePath)
		{
			return Task.Factory.StartNew(() => UpdateStudentScripts(studentScriptsPath, type, scoreFilePath));
		}

		public static Task UpdateStudentScriptsAsync(IEnumerable<ParseResult> studentScripts, string scoreFilePath)
		{
			return Task.Factory.StartNew(() => UpdateStudentScripts(studentScripts, scoreFilePath));
		}

	}
}
