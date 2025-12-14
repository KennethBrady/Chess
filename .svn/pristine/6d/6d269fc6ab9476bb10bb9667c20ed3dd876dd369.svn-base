using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sql.Lib.Scoring.IO
{
	public static class SummaryWorkbookGenerator
	{
		public const string XLSXExtension = ".xlsx";
		public const string SummaryWorkbookName = "Summary Scores.xlsx";

		public static void GenerateSummaryWorkbook(IEnumerable<string> xlsxPaths)
		{
			if (xlsxPaths == null || xlsxPaths.Count() == 0) return;
			List<StudentScores> allScores = xlsxPaths.Select(p => LoadScores(p)).ToList();
			string folder = Path.GetDirectoryName(xlsxPaths.First());
			string path = Path.Combine(folder, SummaryWorkbookName);
			if (File.Exists(path)) File.Delete(path);
			GenerateSummaryWorkbook(path, allScores);
		}

		public static Task GenerateSummaryWorkbookAsync(IEnumerable<string> xlsxPaths)
		{
			return Task.Factory.StartNew(() => GenerateSummaryWorkbook(xlsxPaths));
		}

		private static void GenerateSummaryWorkbook(string fpath, List<StudentScores> scores)
		{
			using (SLDocument doc = new SLDocument())
			{
				SLStyle hdrStyle = doc.CreateStyle();
				hdrStyle.Fill.SetPattern(DocumentFormat.OpenXml.Spreadsheet.PatternValues.Solid, Color.Navy, Color.White);
				hdrStyle.Font.FontColor = Color.White;
				hdrStyle.Font.Bold = true;
				SLStyle boldStyle = doc.CreateStyle();
				boldStyle.Font.Bold = true;
				doc.SetCellValue(2, 1, "Name");
				doc.SetCellValue(2, 2, "Student");
				doc.SetCellValue(2, 3, "Net Score");
				for (int i = 1; i <= 3; ++i) doc.SetCellStyle(2, i, hdrStyle);
				scores.Sort();
				scores.Reverse();
				SLStyle netStyle = doc.CreateStyle();
				netStyle.Font.Bold = true;
				netStyle.SetPatternFill(DocumentFormat.OpenXml.Spreadsheet.PatternValues.Solid, Color.FromArgb(180, 199, 220), Color.Black);
				int row = 0;
				for (int i = 0; i < scores.Count; ++i)
				{
					StudentScores ss = scores[i];
					row = i + 3;
					doc.SetCellValue(row, 1, ss.StudentName);
					doc.SetCellValue(row, 2, $"Student {i + 1}");
					doc.SetCellValue(row, 3, ss.NetScore);
					doc.SetCellStyle(row, 3, netStyle);
				}
				doc.SetCellStyle(row + 1, 3, netStyle);
				int nCol = 4, nDdlCol = 0, nDqlCol = 0;
				if (scores.Any(ss => ss.HasSchemaScores))
				{
					WriteScoreSummaryTable(doc, scores, SqlType.DDL, 3, nCol);
					nDdlCol = nCol;
					nCol += scores.Max(ss => ss.SchemaScores.Count());
				}
				if (scores.Any(ss => ss.HasQueryScores))
				{
					WriteScoreSummaryTable(doc, scores, SqlType.DQL, 3, nCol);
					nDqlCol = nCol;
					nCol += scores.Max(ss => ss.QueryScores.Count());
				}
				if (nDqlCol > 0)
				{
					doc.SetCellValue(1, nDqlCol, "Data Query Language");
					hdrStyle.SetPatternFill(DocumentFormat.OpenXml.Spreadsheet.PatternValues.Solid, Color.DarkMagenta, Color.White);
					doc.SetCellStyle(1, nDqlCol, hdrStyle);
					doc.MergeWorksheetCells(1, nDqlCol, 1, nCol - 1);
				}
				if (nDdlCol > 0)
				{
					doc.SetCellValue(1, nDdlCol, "Data Definition Language");
					hdrStyle.SetPatternFill(DocumentFormat.OpenXml.Spreadsheet.PatternValues.Solid, Color.Red, Color.White);
					doc.SetCellStyle(1, nDdlCol, hdrStyle);
					if (nDqlCol == 0) nDqlCol = nCol;
					doc.MergeWorksheetCells(1, nDdlCol, 1, nDqlCol - 1);
				}
				hdrStyle = doc.CreateStyle();
				hdrStyle.Alignment.Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Center;
				hdrStyle.Alignment.Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center;
				hdrStyle.Font.Bold = true;
				doc.SetCellStyle(2, 4, 2, nCol - 1, hdrStyle);
				SLStyle numStyle = doc.CreateStyle();
				numStyle.FormatCode = "##0.00";
				numStyle.Alignment.Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Center;
				for (int c = 3; c < nCol; ++c)
				{
					for (int r = 0; r < scores.Count; ++r)
					{
						doc.SetCellStyle(r + 3, c, numStyle);
					}
				}
				char ccol = 'C';
				int nStudents = scores.Count;
				doc.SetCellValue(3 + nStudents, 2, "Average:");
				SLStyle style = doc.CreateStyle();
				style.Alignment.Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Right;
				style.Font.Bold = true;
				doc.SetCellStyle(3 + nStudents, 2, style);
				for (int c = 3; c < nCol; ++c)
				{
					doc.SetCellValue(3 + nStudents, c, $"=AVERAGE({ccol}3:{ccol}{nStudents + 2})");
					doc.SetCellStyle(3 + nStudents, c, numStyle);
					ccol = (char)(1 + ((int)ccol));
				}
				doc.AutoFitColumn(1, 3);
				doc.SetColumnWidth(4, nCol - 1, 10);
				doc.SaveAs(fpath);

			}
		}

		private static void WriteScoreSummaryTable(SLDocument doc, List<StudentScores> scores, SqlType type, int startRow, int startColumn)
		{
			int nStudent = 0, nScore = 0;
			foreach (StudentScores sc in scores)
			{
				List<ExerciseScore> ess = (type == SqlType.DQL) ? sc.QueryScores : sc.SchemaScores;
				nScore = 0;
				foreach (ExerciseScore es in ess)
				{
					if (nStudent == 0) doc.SetCellValue(2, startColumn + nScore, es.ExerciseName);
					doc.SetCellValue(startRow + nStudent, startColumn + nScore, es.Score);
					nScore++;
				}
				nStudent++;
			}
			SLStyle style = doc.CreateStyle();
			Color c = (type == SqlType.DDL) ? Color.FromArgb(255, 215, 215) : Color.FromArgb(224, 194, 205);
			style.SetPatternFill(DocumentFormat.OpenXml.Spreadsheet.PatternValues.Solid, c, Color.Black);
			doc.SetCellStyle(startRow - 1, startColumn, startRow + nStudent, startColumn + nScore - 1, style);
		}

		private static StudentScores LoadScores(string xlsxPath)
		{
			StudentScores ss = new StudentScores(Path.GetFileNameWithoutExtension(xlsxPath));
			using (SLDocument document = new SLDocument(xlsxPath))
			{
				if (!document.SelectWorksheet("Summary")) throw new Exception("Workbook is missing the Summary worksheet.");
				string hdr = document.GetCellValueAsString(1, 1);
				int nCol = 1;
				if (hdr == "Schema")
				{
					LoadScores(document, ss.SchemaScores, nCol);
					nCol += 3;
					LoadScores(document, ss.QueryScores, nCol);
				}
				else LoadScores(document, ss.QueryScores, nCol);
				ss.NetScore = document.GetCellValueAsDouble(2, nCol + 3);
			}
			return ss;
		}

		private static void LoadScores(SLDocument document, List<ExerciseScore> scores, int startColumn)
		{
			int nRow = 2;
			do
			{
				string name = document.GetCellValueAsString(nRow, startColumn), sScore = document.GetCellValueAsString(nRow, startColumn + 1);
				if (name == "Sum:" || string.IsNullOrEmpty(name)) break;
				if (!double.TryParse(sScore, out double netScore)) break;
				scores.Add(new ExerciseScore
				{
					ExerciseName = name,
					Score = netScore
				});
				nRow++;
			} while (true);
		}

		private class StudentScores : IComparable<StudentScores>
		{
			public StudentScores(string studentName)
			{
				StudentName = studentName;
				SchemaScores = new List<ExerciseScore>();
				QueryScores = new List<ExerciseScore>();
			}

			public string StudentName { get; private set; }
			public List<ExerciseScore> SchemaScores { get; private set; }
			public bool HasSchemaScores => SchemaScores.Count > 0;
			public List<ExerciseScore> QueryScores { get; private set; }
			public bool HasQueryScores => QueryScores.Count > 0;
			public double NetScore { get; set; }

			int IComparable<StudentScores>.CompareTo(StudentScores other)
			{
				return Comparer<double>.Default.Compare(NetScore, other.NetScore);
			}
		}

		private class ExerciseScore
		{
			public string ExerciseName { get; set; }
			public double Score { get; set; }
		}
	}
}
