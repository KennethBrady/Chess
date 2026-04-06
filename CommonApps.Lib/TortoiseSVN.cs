using CommonApps.Lib.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonApps.Lib
{
	public static class TortoiseSVN
	{
		public static readonly string InstallDir = @"C:\Program Files\TortoiseSVN\";
		public static readonly string ExeDir = @"C:\Program Files\SlikSvn\bin\";
		public static readonly string SVNPath = string.Empty;
		static TortoiseSVN()
		{
			if (Directory.Exists(InstallDir))
			{
				string svnPath = Path.Combine(ExeDir, "svn.exe");
				if (File.Exists(svnPath)) SVNPath = svnPath;
			}
		}

		public static bool IsInstalled => Directory.Exists(InstallDir) && File.Exists(SVNPath);

		public static async Task<bool> IsVersionControlled(string folderPath)
		{
			VerifyInstalled();
			if (!Directory.Exists(folderPath)) throw new ArgumentException($"Folder '{folderPath}' not found.");
			var result = await Runner.Execute(SVNPath, folderPath, "info");
			return result.ExitCode == 0 && result.Output.Contains("Revision");
		}

		public static async Task<SVNInfo> GetInfo(string path)
		{
			VerifyInstalled();
			var result = await Runner.Execute(SVNPath, path, "info");
			if (result.ExitCode != 0) throw new Exception($"SVN exited with error code {result.ExitCode}: {result.Errors}");
			return new SVNInfo(result.LineOutput);
		}

		[Obsolete("Use UpdateFolderWithSummary")]
		public static async Task<(bool success, string output)> UpdateFolder(string folder)
		{
			VerifyInstalled();
			if (!Directory.Exists(folder)) throw new ArgumentException($"Folder '{folder}' not found");
			bool vControlled = await IsVersionControlled(folder);
			if (!vControlled) throw new ArgumentException($"Folder '{folder}' is not under version control.");
			try
			{
				var result = await Runner.Execute(SVNPath, folder, "update");
				return (result.ExitCode == 0 && result.LineOutput.StartsWith("Updating"), result.LineOutput);
			}
			catch
			{
				return (false, string.Empty);
			}
		}

		public static async Task<UpdateSummary> UpdateFolderWithSummary(string folder)
		{
			// https://svnbook.red-bean.com/en/1.7/svn.ref.svn.c.update.html
			VerifyInstalled();
			if (!Directory.Exists(folder)) throw new ArgumentException($"Folder '{folder}' not found");
			bool vControlled = await IsVersionControlled(folder);
			if (!vControlled) throw new ArgumentException($"Folder '{folder}' is not under version control.");
			string parent = Directory.GetParent(folder)!.FullName;
			var result = await Runner.Execute(SVNPath, parent, "update", Path.GetFileName(folder));
			if (!result.Succeeded) throw new Exception("SVN failed: " + result.Errors);
			return new UpdateSummary(result.LineOutput);
		}

		private static void VerifyInstalled()
		{
			if (!IsInstalled) throw new InvalidOperationException($"{nameof(TortoiseSVN)} is not installed.");
		}

		public enum NodeKind { Directory, File};

		public struct SVNInfo
		{
			internal SVNInfo(string svnOutput)
			{
				SVNOutput = svnOutput;
				string[] lines = svnOutput.Split(Environment.NewLine);
				foreach(string l in lines)
				{
					int cpos = l.IndexOf(':');
					if (cpos > 1)
					{
						string name = l.Substring(0, cpos).Trim(), val = l.Substring(cpos + 1).Trim();
						switch(name)
						{
							case "Path":	Path = val; break;
							case "Working Copy Root Path": WorkingCopyRootPath = val; break;
							case "URL":	URL = val; break;
							case "Repository Root": RepositoryRoot = val; break;
							case "Repository UUID":	RepositoryUUID = val; break;
							case "Revision":	Revision = int.Parse(val); break;
							case "Node Kind":	
								switch(val)
								{
									case "directory": NodeKind = NodeKind.Directory; break;
									case "file":	NodeKind = NodeKind.File; break;
								}
								break;
							case "Last Changed Rev":	LastChangedRevision = int.Parse(val); break;
							case "Last Changed Date":
								int opPos = val.IndexOf(" (");
								string dts = val.Substring(0, opPos);
								LastChangedDate = DateTime.Parse(dts);
								break;
						}
					}
				}
			}

			public string SVNOutput { get; private init; }
			public string Path { get; private init; } = string.Empty;
			public string WorkingCopyRootPath { get; private init; } = string.Empty;
			public string URL { get; private init; } = string.Empty;
			public string RepositoryRoot { get; private init; } = string.Empty;
			public string RepositoryUUID { get; private init; } = string.Empty;
			public int Revision { get; private init; } = 0;
			public NodeKind NodeKind { get; private init; } = NodeKind.Directory;
			public int LastChangedRevision { get; private init; } = 0;
			public DateTime LastChangedDate { get; private init; } = DateTime.MinValue;
		}

		public class UpdateSummary : IEnumerable<UpdateRecord>
		{
			private static readonly Regex _rxUpdate = new Regex(@"(?<=Updating ')(.+)(?=':)", RegexOptions.Compiled);
			private static readonly Regex _rxRevision = new Regex(@"(?<=revision )(\d+)", RegexOptions.Compiled);
			private const string _sRxExternal = @"(?<=Fetching external item into 'FOLDER\\)(.+)(?=':)";
			private List<UpdateRecord> _updates = new List<UpdateRecord>();

			internal UpdateSummary(string output)
			{
				SVNOutput = output;
				Match m = _rxUpdate.Match(output);
				if (!m.Success) throw new ArgumentException("Unexpected SVN Update output");
				FolderName = m.Value;
				Regex rxExt = new Regex(_sRxExternal.Replace("FOLDER", FolderName));
				MatchCollection matches = rxExt.Matches(output);
				IndexedStrings lines = new IndexedStrings(output);
				var lRev = lines.Last(l => l.Value.StartsWith("At revision"));
				m = _rxRevision.Match(lRev.Value);
				if (!m.Success) throw new ArgumentException("Unable to find final revision in SVN Update output");
				Revision = int.Parse(m.Value);
				foreach (Match match in matches)
				{
					m = _rxRevision.Match(output, match.Index);
					IndexedStrings.Line? l1 = lines[match.Index], l2 = lines[m.Index];
					if(l1 != null && l2 != null) _updates.Add(new UpdateRecord(match.Value, int.Parse(m.Value), lines.Lines.Skip(l1.LineNumber + 1).Take(l2.LineNumber - l1.LineNumber)));
				}

			}

			public string SVNOutput { get; private init; }
			public string FolderName { get; private init; }
			public int Revision { get; private init; }
			public IReadOnlyList<UpdateRecord> Updates => _updates;
			public IEnumerable<string> UpdatedItems => _updates.SelectMany(u => u.UpdatedItems);
			public IEnumerable<string> DeletedItems => _updates.SelectMany(u => u.DeletedItems);
			public IEnumerable<string> AddedItems => _updates.SelectMany(u => u.AddedItems);
			public IEnumerable<string> MergedItems => _updates.SelectMany(u => u.MergedItems);
			public IEnumerable<string> ConflictedItems => _updates.SelectMany(u => u.ConflictedItems);

			IEnumerator<UpdateRecord> IEnumerable<UpdateRecord>.GetEnumerator() => _updates.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => _updates.GetEnumerator();
		}

		public class UpdateRecord
		{
			private static readonly List<string> _empty = new List<string>(1);

			private List<string>? _added, _deleted, _updated, _merged, _conflicted;

			internal UpdateRecord(string external, int revision, IEnumerable<string> items)
			{
				ExternalItem = external;
				Revision = revision;
				void add(ref List<string>? list, string v)
				{
					int n = v.LastIndexOf('\\');
					if (n < 0) return;
					v = v.Substring(n + 1);
					if (list == null) list = new List<string>();
					list.Add(v);
				}
				foreach(string item in items)
				{
					if (string.IsNullOrEmpty(item)) continue;
					switch(item[0])
					{
						case 'D':	add(ref _deleted!, item); break;
						case 'A':	add(ref _added!, item); break;
						case 'U':	add(ref _updated!, item); break;
						case 'C':	add(ref _conflicted!, item); break;
						case 'G':	add(ref _merged!, item); break;
					}
				}
			}

			public string ExternalItem { get; private init; }
			public int Revision { get; private init; }
			public IReadOnlyList<string> AddedItems => (_added == null) ? _empty.AsReadOnly() : _added.AsReadOnly();
			public IReadOnlyList<string> DeletedItems => (_deleted == null) ? _empty.AsReadOnly() : _deleted.AsReadOnly();
			public IReadOnlyList<string> UpdatedItems => (_updated == null) ? _empty.AsReadOnly() : _updated.AsReadOnly();
			public IReadOnlyList<string> MergedItems => (_merged == null) ? _empty.AsReadOnly() : _merged.AsReadOnly();
			public IReadOnlyList<string> ConflictedItems => (_conflicted == null) ? _empty.AsReadOnly() : _conflicted.AsReadOnly();
			public int TotalItemCount => AddedItems.Count + DeletedItems.Count + UpdatedItems.Count + MergedItems.Count + ConflictedItems.Count;
		}

		private class IndexedStrings : IEnumerable<IndexedStrings.Line>
		{
			private List<Line> _lines = new List<Line>();

			public IndexedStrings(string content)
			{
				OriginalContent = content;
				if (string.IsNullOrEmpty(content)) return;
				string[] lines = content.Split(Environment.NewLine);
				int pos = 0, n = 0;
				foreach (string l in lines)
				{
					Line ll = new Line(l, pos, n++);
					_lines.Add(ll);
					pos += (ll.Length + 2);
				}
			}

			public string OriginalContent { get; private init; }

			public Line? this[int characterIndex]
			{
				get
				{
					return _lines.FirstOrDefault(l => l.StartIndex <= characterIndex && l.EndIndex >= characterIndex);
				}
			}

			public IEnumerable<string> Lines => _lines.Select(l => l.Value);

			IEnumerator<Line> IEnumerable<IndexedStrings.Line>.GetEnumerator()
			{
				return ((IEnumerable<Line>)_lines).GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable)_lines).GetEnumerator();
			}

			public class Line
			{
				internal Line(string value, int startIndex, int lineNumber)
				{
					Value = value;
					StartIndex = startIndex;
					EndIndex = StartIndex + Value.Length;
					LineNumber = lineNumber;
				}

				public string Value { get; private init; }
				public int Length => Value.Length;
				public int LineNumber { get; private init; }
				public int StartIndex { get; private init; }
				public int EndIndex { get; private init; }

				public override string ToString() => $"[{StartIndex:N0}-{EndIndex:N0}]: {Value}";

			}
		}

	}
}
