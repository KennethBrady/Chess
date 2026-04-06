using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonApps.Lib
{
	public static class ExifTool
	{
		public const string ExePath = @"C:\Sys\exiftool.exe";

		public static bool IsInstalled => File.Exists(ExePath);

		public class ExifResult
		{
			public static readonly ExifResult Empty = new ExifResult();

			internal ExifResult(string fpath)
			{
				FilePath = fpath;
				FileCreatedDate = File.GetCreationTime(fpath);
			}

			private ExifResult()
			{
				FilePath = string.Empty;
				FileCreatedDate = DateTime.MaxValue;
			}

			public string FilePath { get; init; }
			public bool IsEmpty => string.IsNullOrEmpty(FilePath);
			private DateTime FileCreatedDate { get; init; }
			public DateTime? FileModificationDate { get; internal set; }
			public DateTime? FileAccessDate { get; internal set; }
			public DateTime? FileCreationDate { get; internal set; }
			public DateTime? MediaCreationDate { get; internal set; }

			private static DateTime Min(DateTime? d1, DateTime? d2)
			{
				if (!d1.HasValue && !d2.HasValue) return DateTime.MaxValue;
				if (d1.HasValue && !d2.HasValue) return d1.Value;
				if (d2.HasValue && !d1.HasValue) return d2.Value;
				return (d1!.Value < d2!.Value) ? d1.Value : d2.Value;
			}

			public DateTime EarliestKnownDate() => Min(FileModificationDate, Min(FileAccessDate, Min(FileCreationDate, Min(MediaCreationDate, FileCreatedDate))));

		}

		private static readonly Regex _dtRx = new Regex(@"(\d+)", RegexOptions.Compiled);
		public static ExifResult ScanFile(string filePath)
		{
			if (!IsInstalled || !File.Exists(filePath)) return ExifResult.Empty;
			Process p = new Process();
			p.StartInfo.FileName = ExePath;
			p.StartInfo.Arguments = filePath;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			p.StartInfo.CreateNoWindow = true;
			p.Start();
			string output = p.StandardOutput.ReadToEnd();
			p.WaitForExit();
			string[] lines = output.Split(Environment.NewLine);
			ExifResult r = new ExifResult(filePath);
			DateTime parse(string s)
			{
				MatchCollection m = _dtRx.Matches(s);
				int year = int.Parse(m[0].Value), month = int.Parse(m[1].Value), day = int.Parse(m[2].Value),
					h = int.Parse(m[3].Value), min = int.Parse(m[4].Value), sec = int.Parse(m[5].Value);
				return new DateTime(year, month, day, h, min, sec);
			}
			foreach (string line in lines)
			{
				string[] parts = line.Split(" : ");

				if (parts.Length == 2)
				{
					switch (parts[0].Trim())
					{
						case "File Modification Date/Time": r.FileModificationDate = parse(parts[1]); break;
						case "File Creation Date/Time": r.FileCreationDate = parse(parts[1]); break;
						case "File Access Date/Time": r.FileAccessDate = parse(parts[1]); break;
						case "Media Create Date": r.MediaCreationDate = parse(parts[1]); break;
					}
				}
			}
			return r;
		}

		public static Task<ExifResult> ScanFileAsync(string filePath)
		{
			ExifResult scan() => ScanFile(filePath);
			return Task<ExifResult>.Factory.StartNew(scan);
		}
	}
}
