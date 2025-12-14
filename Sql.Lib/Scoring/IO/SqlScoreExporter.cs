using Chamilo.Lib.IO;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using Tuple = System.Tuple;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Sql.Lib.Scoring.IO
{
	public class SqlScoreExporter
	{
		static SqlScoreExporter()
		{
			XLSX.OneBasedColumns = true;
		}

		private const string DDLSheetName = "Schema Definitions";
		private const string DQLSheetName = "Queries";
		private const string SummarySheetName = "Summary";
		private string _fpath;
		private Dictionary<SqlType, Dictionary<string, Tuple<double, double>>> _summaries;
		private SLDocument Document;

		public SqlScoreExporter(string fpath)
		{
			_fpath = fpath;
			if (File.Exists(fpath)) Document = new SLDocument(fpath);
			else
			{
				Document = new SLDocument();
				Document.SaveAs(fpath);
				// WORKAROUND - close/reopen to prevent exception in CreateStyles.
				Document.Dispose();
				System.Threading.Thread.Sleep(200);
				Document = new SLDocument(fpath);
			}
			CreateStyles();
			_summaries = new Dictionary<SqlType, Dictionary<string, Tuple<double, double>>>();
			_summaries.Add(SqlType.DDL, new Dictionary<string, Tuple<double, double>>());
			_summaries.Add(SqlType.DQL, new Dictionary<string, Tuple<double, double>>());
		}

		//public void WriteScores(IEnumerable<ISqlScorable> scores)
		//{
		//	if (scores == null) throw new ArgumentNullException(nameof(scores));
		//	if (scores.Count() == 0) return;
		//	WriteScores(scores.Where(s => s.Type == SqlType.DDL), DDLSheetName);
		//	WriteScores(scores.Where(s => s.Type == SqlType.DQL), DQLSheetName);
		//	WriteSummary();
		//	Finish();
		//	Document.Save();
		//}

		public void WriteScores(params ISqlScoreSet[] scores)
		{
			if (scores.Length == 0) return;
			string ddlComments = null, dqlComments = null;
			foreach(var scoreset in scores)
			{
				string sheetName = null;
				switch(scoreset.Type)
				{
					case SqlType.DDL:
						sheetName = DDLSheetName;
						ddlComments = scoreset.Comments;
						break;
					case SqlType.DQL:
						sheetName = DQLSheetName;
						dqlComments = scoreset.Comments;
						break;
					default:	throw new NotSupportedException($"SqlType.{scoreset.Type} is not supported.");
				}
				WriteScores(scoreset.Scores, sheetName);
			}
			WriteSummary(ddlComments, dqlComments);
			Finish();
			Document.Save();
		}

		private SLStyle CorrectStyle { get; set; }
		private SLStyle IncorrectStyle { get; set; }
		private SLStyle RightBoldStyle { get; set; }
		private SLStyle RightStyle { get; set; }
		private SLStyle BoldStyle { get; set; }
		private SLStyle CenterStyle { get; set; }
		private SLStyle VCenterStyle { get; set; }
		private SLStyle VTopStyle { get; set; }
		private SLStyle CenterBoldStyle { get; set; }
		private SLStyle UnderBorderStyle { get; set; }
		private SLStyle LightUnderBorderStyle { get; set; }
		private SLStyle WrapStyle { get; set; }

		private void CreateStyles()
		{
			CorrectStyle = Document.CreateStyle();
			IncorrectStyle = Document.CreateStyle();
			CorrectStyle.Fill.SetPattern(PatternValues.Solid, Color.LightGreen, Color.Black);
			IncorrectStyle.Fill.SetPattern(PatternValues.Solid, Color.Pink, Color.Black);
			RightBoldStyle = Document.CreateStyle();
			RightBoldStyle.Alignment.Horizontal = HorizontalAlignmentValues.Right;
			RightBoldStyle.Font.Bold = true;
			RightStyle = Document.CreateStyle();
			RightStyle.Alignment.Horizontal = HorizontalAlignmentValues.Right;
			BoldStyle = Document.CreateStyle();
			BoldStyle.Font.Bold = true;
			CenterStyle = Document.CreateStyle();
			CenterStyle.Alignment.Horizontal = HorizontalAlignmentValues.Center;
			CenterBoldStyle = Document.CreateStyle();
			CenterBoldStyle.Alignment.Horizontal = HorizontalAlignmentValues.Center;
			CenterBoldStyle.Font.Bold = true;
			UnderBorderStyle = Document.CreateStyle();
			UnderBorderStyle.Border.SetBottomBorder(BorderStyleValues.Thick, Color.Black);
			LightUnderBorderStyle = Document.CreateStyle();
			LightUnderBorderStyle.Border.SetBottomBorder(BorderStyleValues.Thin, Color.Black);
			VCenterStyle = Document.CreateStyle();
			VCenterStyle.Alignment.Vertical = VerticalAlignmentValues.Center;
			VTopStyle = Document.CreateStyle();
			VTopStyle.Alignment.Vertical = VerticalAlignmentValues.Top;
			WrapStyle = Document.CreateStyle();
			WrapStyle.SetWrapText(true);
			WrapStyle.Alignment.Vertical = VerticalAlignmentValues.Top;
		}

		private void AddOrReplaceSheet(string sheetName)
		{
			if (HasSheet(sheetName)) Document.DeleteWorksheet(sheetName);
			Document.AddWorksheet(sheetName);
			Document.SelectWorksheet(sheetName);
		}

		private bool HasSheet(string sheetName)
		{
			return Document.GetSheetNames().Contains(sheetName);
		}

		private void WriteScores(IEnumerable<ISqlScoreExportable> scores, string sheetName)
		{
			if (scores.Count() == 0) return;
			AddOrReplaceSheet(sheetName);
			int startRow = 1, n = 0;
			foreach (var sql in scores) WriteScores(sql, n++, ref startRow);
			Document.AutoFitColumn(1, 4);
			Document.AutoFitColumn(SQLColumn);
			Document.SetColumnWidth(SQLColumn + 1, 65.0);
			Document.AutoFitColumn(SQLColumn + 2);
			Document.SetColumnWidth(SQLColumn + 3, 65.0);
		}

		private void WriteScores(ISqlScoreExportable scorable, int index, ref int startRow)
		{
			SLDocument doc = Document;
			var calc = XLSX.CreateCellCalculator(startRow);
			SLStyle style = Document.CreateStyle();
			style.Font.FontColor = Color.White;
			style.Font.Bold = true;
			style.Fill.SetPattern(PatternValues.Solid, Color.Navy, Color.White);
			Document.SetCellValue(calc[0, 0], scorable.Name);
			Document.SetCellStyle(calc[0, 0], calc[3, 0], style);
			Document.MergeWorksheetCells(calc[0, 0], calc[3, 0]);

			calc.StartRow++;
			double sum = 0, wsum = 0;
			if (scorable.OverrideScoring)
			{
				calc.StartRow++;
				Document.SetCellValue(calc[2, 0], "Net Score:");
				Document.SetCellValue(calc[3, 0], $"{scorable.CustomScore}");
				sum += scorable.CustomScore;
				wsum += scorable.Scores.Where(s => s.IncludeScore).Sum(s => s.Weight);
			} else
			{
				Document.SetCellValue(calc[0, 0], "Property");
				Document.SetCellValue(calc[1, 0], "Expected");
				Document.SetCellValue(calc[2, 0], "Observed");
				Document.SetCellValue(calc[3, 0], "Score");
				style = doc.CreateStyle();
				style.Border.BottomBorder.BorderStyle = BorderStyleValues.Thin;
				style.Font.Bold = true;
				style.Fill.SetPattern(PatternValues.Solid, Color.LightSkyBlue, Color.Black);
				doc.SetCellStyle(calc[0, 0], calc[3, 0], style);
				foreach (var item in scorable.Scores.Where(s => s.IncludeScore))
				{
					calc.StartRow++;
					Document.SetCellValue(calc[0, 0], item.Name);
					Document.SetCellValue(calc[1, 0], (item.ExpectedValue == null) ? string.Empty : item.ExpectedValue.ToString());
					Document.SetCellValue(calc[2, 0], (item.ObservedValue == null) ? string.Empty : item.ObservedValue.ToString());
					Document.SetCellValue(calc[3, 0], item.Score);
					Document.SetCellStyle(calc[3, 0], CenterStyle);
					if (item.IsCorrect) Document.SetCellStyle(calc[0, 0], calc[3, 0], CorrectStyle);
					else Document.SetCellStyle(calc[0, 0], calc[3, 0], IncorrectStyle);
					sum += item.Score;
					wsum += item.Weight;
				}
				calc.StartRow++;
				Document.SetCellValue(calc[2, 0], "Total:");
				Document.SetCellStyle(calc[2, 0], RightBoldStyle);
				Document.SetCellValue(calc[3, 0], $"{sum:N0} / {wsum:N0}");
				Document.SetCellStyle(calc[3, 0], CenterBoldStyle);
			}
			AddSummary(scorable.Name, scorable.Type, sum, wsum);
			if (!string.IsNullOrEmpty(scorable.Instructions))
			{
				Document.SetCellValue(startRow, SQLColumn, "Instructions:");
				Document.SetCellValue(startRow, SQLColumn + 1, scorable.Instructions);
				Document.MergeWorksheetCells(startRow, SQLColumn + 1, startRow + 1, SQLColumn + 3);
				Document.SetCellStyle(startRow, SQLColumn + 1, startRow + 1, SQLColumn + 3, VTopStyle);
				startRow += 2;
			}

			Document.SetCellValue(startRow, SQLColumn, "Your SQL");
			Document.SetCellValue(startRow, SQLColumn + 1, scorable.Script);
			Document.MergeWorksheetCells(startRow, SQLColumn, startRow + 6, SQLColumn);
			Document.MergeWorksheetCells(startRow, SQLColumn + 1, startRow + 6, SQLColumn + 1);
			Document.SetCellStyle(startRow, SQLColumn, startRow, SQLColumn + 1, VCenterStyle);

			Document.SetCellValue(startRow, SQLColumn + 2, "Reference SQL:");
			Document.SetCellValue(startRow, SQLColumn + 3, scorable.RefScript);
			Document.MergeWorksheetCells(startRow, SQLColumn + 2, startRow + 6, SQLColumn + 2);
			Document.MergeWorksheetCells(startRow, SQLColumn + 3, startRow + 6, SQLColumn + 3);
			Document.SetCellStyle(startRow, SQLColumn + 2, startRow, SQLColumn + 3, VCenterStyle);

			if (!string.IsNullOrEmpty(scorable.Comments))
			{
				Document.SetCellValue(calc[0], "Comments:");
				Document.SetCellStyle(calc[0], BoldStyle);
				Document.SetCellValue(calc[0, 1], scorable.Comments);
				Document.MergeWorksheetCells(calc[0, 1], calc[2, 3]);
				Document.SetCellStyle(calc[0, 1], calc[2, 3], WrapStyle);
			}
			startRow += scorable.Scores.Count() + 6;
		}

		const int SQLColumn = 5;

		private void AddSummary(string scriptName, SqlType type, double score, double maxScore)
		{
			var dict = _summaries[type];
			dict.Add(scriptName, Tuple.Create(score, maxScore));
		}

		private string PercentScore(double sum, double wsum)
		{
			double pcnt = 100 * sum / wsum;
			return $"{sum:N0} / {wsum:N0} ({pcnt:N1}%)";
		}

		private void WriteSummary(string ddlComments, string dqlComments)
		{
			AddOrReplaceSheet(SummarySheetName);
			var calc = XLSX.CreateCellCalculator(1, 1);
			Document.SetCellValue(1, 1, "Net Score:");
			var sumsS = WriteSummary(SqlType.DDL, _summaries[SqlType.DDL], 4, ddlComments);
			var sumsQ = WriteSummary(SqlType.DQL, _summaries[SqlType.DQL], 8  + _summaries[SqlType.DDL].Count, dqlComments);
			double sum = sumsS.Item1 + sumsQ.Item1, wsum = sumsS.Item2 + sumsQ.Item2;
			double pcnt = 100 * sum / wsum;
			Document.SetCellValue(1, 2, PercentScore(sum, wsum));
			Document.SetCellStyle(1, 1, RightBoldStyle);
			Document.SetCellStyle(1, 2, BoldStyle);
			Document.SetColumnStyle(2, CenterStyle);
			Document.AutoFitColumn(1);
			Document.SetColumnWidth(2, 25);
			Document.SetActiveCell(2, 1);
		}

		private Tuple<double,double> WriteSummary(SqlType type, Dictionary<string, Tuple<double,double>> scores, int startRow, string comments)
		{
			double sum = 0, wsum = 0;
			string colHeader = type == SqlType.DDL ? "Schemas" : "Queries";
			Document.SetCellValue(startRow, 1, colHeader);
			Document.MergeWorksheetCells(startRow, 1, startRow, 2);
			Document.SetCellStyle(startRow, 1, CenterBoldStyle);
			Document.SetCellStyle(startRow, 1, LightUnderBorderStyle);
			int nRow = startRow;
			foreach(var item in scores)
			{
				Document.SetCellValue(++nRow, 1, item.Key);
				Document.SetCellStyle(nRow, 1, RightStyle);
				sum += item.Value.Item1;
				wsum += item.Value.Item2;
				Document.SetCellValue(nRow, 2, $"{item.Value.Item1:N0} / {item.Value.Item2:N0}");
			}
			Document.SetCellStyle(nRow, 1, nRow, 2, UnderBorderStyle);
			Document.SetCellValue(++nRow, 1, "Net Score:");
			Document.SetCellValue(nRow, 2, PercentScore(sum, wsum));
			Document.SetCellStyle(nRow, 1, RightBoldStyle);
			Document.SetCellStyle(nRow, 2, BoldStyle);
			if (!string.IsNullOrEmpty(comments))
			{
				Document.SetCellValue(startRow + 1, 3, comments);
				Document.MergeWorksheetCells(startRow + 1, 3, startRow + scores.Count, 3);
				Document.SetColumnWidth(3, 50);
				Document.SetCellStyle(startRow + 1, 3, VCenterStyle);
				Document.SetCellStyle(startRow + 1, 3, WrapStyle);
			}
			return Tuple.Create(sum, wsum);
		}

		private void Finish()
		{
			Document.DeleteWorksheet("Sheet1");
			Document.MoveWorksheet(SummarySheetName, 1);
			Document.SelectWorksheet(SummarySheetName);
		}
	}
}
