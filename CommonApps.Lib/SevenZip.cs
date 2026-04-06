using Common.Lib.Contracts;
using Common.Lib.IO;
using CommonApps.Lib.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonApps.Lib
{
	public static class SevenZip
	{
		// https://7zip.bugaco.com/7zip/MANUAL/cmdline/syntax.htm
		private static readonly Random _random = Random.Shared;
		private const string SQ = "\"";
		private static readonly string _installFolder;
		public static string InstallFolder => _installFolder;
		public static bool IsInstalled => Directory.Exists(InstallFolder);
		public static string ExecutablePath => IsInstalled ? Path.Combine(InstallFolder, "7z.exe") : string.Empty;
		public static string GUIExecutablePath => IsInstalled ? Path.Combine(InstallFolder, "7zG.exe") : string.Empty;
		public const string FileExtension = ".7z";
		public const string FileExtensionAsWildcard = "*.7z";
		static SevenZip()
		{
			string folder = Path.Combine(AppCommon.ProgramFiles, "7-Zip");
			if (Directory.Exists(folder))
			{
				_installFolder = folder;
			}
			else _installFolder = string.Empty;
		}

		private static void VerifyInstalled()
		{
			if (!IsInstalled) throw new ApplicationException("Cannot run 7zip because it is not installed.");
		}

		public static bool IsArchiveFile(string fileNameOrPath)
		{
			string ext = Path.GetExtension(fileNameOrPath).ToLower();
			return ext == ".7z" || ext == ".zip";
		}

		public static Task<ConsoleResult> ArchiveFolder(string sourceFolder, string outputPath,
			params string[] excludeFiles)
		{
			return ArchiveFolder(sourceFolder, outputPath, 0, excludeFiles);
		}

		public static async Task<ConsoleResult> ArchiveFolder(string sourceFolder, string outputPath,
			int processPriority, params string[] excludeFiles)
		{
			VerifyInstalled();
			if (!Directory.Exists(sourceFolder)) throw new ArgumentException($"'{sourceFolder}' does not exist.");
			if (string.IsNullOrEmpty(outputPath)) throw new ArgumentNullException(nameof(outputPath));
			string outFile = $"{_random.Next()}.7z";
			List<string> args = new List<string>() { "a", outFile, "." };
			if (excludeFiles.Length > 0)
			{
				foreach (string xf in excludeFiles)
				{
					args.Add($"-x!{xf}");
				}
			}
			if (File.Exists(outputPath)) File.Delete(outputPath);
			var r = await Runner.Execute(ExecutablePath, sourceFolder, args.ToArray());
			if (r.Succeeded)
			{
				string outPath = Path.Combine(sourceFolder, outFile);
				File.Move(outPath, outputPath);
			}
			return new ConsoleResult();
		}

		public static async Task<ConsoleResult> Extract(string archiveFilePath, string destDir = "", bool recurse = false, bool overwrite = true)
		{
			// 7z e archive.zip -oc:\soft *.cpp -r
			if (!File.Exists(archiveFilePath)) throw new FileNotFoundException(archiveFilePath);
			if (string.IsNullOrEmpty(destDir)) destDir = Environment.CurrentDirectory;
			if (!Directory.Exists(destDir)) throw new DirectoryNotFoundException(destDir);
			if (!destDir.EndsWith('\\')) destDir += "\\";
			List<string> args = new List<string>();
			string fname = Path.GetFileName(archiveFilePath), srcFolder = Path.GetDirectoryName(archiveFilePath)!, 
				destFolder = string.IsNullOrEmpty(destDir) ? string.Empty : Path.GetDirectoryName(destDir)!;
			args.Add("e"); args.Add(fname);
			if (!PathEx.AreSameFolder(srcFolder, destFolder)) args.Add($"-o{destFolder}");
			if (recurse) args.Add("-r");
			if (overwrite) args.Add("-aoa");
			return await Runner.Execute(ExecutablePath, srcFolder, args.ToArray());
		}

		public static async Task<ConsoleResult> ArchiveFolder(string sourceFolder, string outputPath, bool recurse, Action<Progress>? statusUpdate, params string[] excludeFiles)
		{
			VerifyInstalled();
			if (!Directory.Exists(sourceFolder)) throw new ArgumentException($"'{sourceFolder}' does not exist.");
			if (string.IsNullOrEmpty(outputPath)) throw new ArgumentNullException(nameof(outputPath));
			string outFile = $"{_random.Next()}.7z";
			List<string> args = new List<string>();
			args.Add("a");
			if (recurse) args.Add("-r");
			if (statusUpdate != null) args.Add("-bsp1");
			args.Add(outFile);
			if (excludeFiles.Length > 0)
			{
				foreach (string xf in excludeFiles)
				{
					args.Add($"-x!{xf}");
				}
			}
			InputProcessor ip = new InputProcessor(statusUpdate);
			void report(OutputType type, string content) => ip.Process(content);
			var r = await Runner.ExecuteWithFeedback(ExecutablePath, sourceFolder, args, 0, report);
			string fpath = Path.Combine(sourceFolder, outFile);
			if (r.Succeeded)
			{
				File.Move(fpath, outputPath, true);
			} else
			{
				if (File.Exists(fpath))
				{
					try
					{
						File.Delete(fpath);
					}
					catch { }
				}
			}
			return r;
		}

		// No feedback :(
		public static async Task<ConsoleResult> ArchiveFile(string filePath, string workingDir, string outputPath)
		{
			VerifyInstalled();
			if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
			string ext = Path.GetExtension(outputPath);
			using TempFile tempFile = TempFile.InFolderWithExtension(workingDir, ext);
			List<string> args = ["a"];
			args.Add(tempFile.Name);
			args.Add(filePath);
			var r = await Runner.Execute(ExecutablePath, workingDir, args.ToArray());
			if (r.Succeeded) File.Copy(tempFile, outputPath, true);
			return r;
		}

		// No feedback :(
		public static async Task<ConsoleResult> UpdateArchive(string archiveFilePath, string workingDir, string newFilePath)
		{
			VerifyInstalled();
			if (!File.Exists(archiveFilePath)) throw new FileNotFoundException(archiveFilePath);
			if (!File.Exists(newFilePath)) throw new FileNotFoundException(newFilePath);
			List<string> args = ["u"];
			args.Add(Path.GetFileName(archiveFilePath));
			args.Add(newFilePath);
			var r = await Runner.Execute(ExecutablePath, workingDir, args.ToArray());
			return r;
		}

		// NOTE:  7zip with @list-file does not provide real-time output.
		public static async Task<ConsoleResult> ArchiveFiles(IEnumerable<string> files, string workingDir, string outputPath)
		{
			if (!IsInstalled) throw new ApplicationException("Cannot run 7zip because it is not installed.");
			if (!Directory.Exists(workingDir)) throw new DirectoryNotFoundException(workingDir);
			if (files == null) throw new ArgumentNullException(nameof(files));
			if (string.IsNullOrEmpty(outputPath)) throw new ArgumentNullException(nameof(outputPath));
			List<string> args = ["a"];
			using TempFile tmp = TempFile.WithExtension(".txt");
			using StreamWriter w = new StreamWriter(tmp.OpenWrite());
			foreach (string file in files) w.WriteLine(file);
			w.Close();
			args.Add($"-i@{tmp.FilePath}");
			foreach (string file in files)
			{
				string relPath = file.Substring(workingDir.Length);
				string fpath = relPath.Contains(' ') ? $"\"{relPath}\"" : relPath;
				args.Add($"-i{fpath}");
			}
			string outFile = Path.Combine(workingDir, $"{_random.Next()}.7z");
			args.Add(outFile);
			var r = await Runner.Execute(ExecutablePath, workingDir, args.ToArray());
			if (r.Succeeded) File.Move(outFile, outputPath, true); else
			{
				if (File.Exists(outFile))
				{
					try
					{
						File.Delete(outFile);
					}
					catch { }
				}
			}
			return r;
		}

		public static async Task<ArchiveContents> ListContents(string archiveFilePath)
		{
			VerifyInstalled();
			if (!File.Exists(archiveFilePath)) throw new FileNotFoundException(archiveFilePath);
			List<string> lines = new();
			string folder = Path.GetDirectoryName(archiveFilePath)!, name = Path.GetFileName(archiveFilePath);
			string[] args = ["l", name];
			var result = await Runner.ExecuteWithFeedback(ExecutablePath, folder, ["l", name], 0, (type, line) =>
			{
				if (line != null) lines.Add(line);
			});
			if (!result.Succeeded) throw new Exception("7Zip failed with message:" + result.Errors);
			return ArchiveContents.FromOutput(lines);
		}

		public static async Task<ConsoleResult> Extract(string archiveFilePath, string outputFolder, params string[] wildcards)
		{
			VerifyInstalled();
			if (!File.Exists(archiveFilePath)) throw new FileNotFoundException(archiveFilePath);
			List<string> args = ["e"];
			args.Add(archiveFilePath);
			foreach (string wc in wildcards) args.Add(wc);
			args.Add("-y");
			var result = await Runner.Execute(ExecutablePath, outputFolder, args.ToArray());
			return result;
		}

		public static async Task<ConsoleResult> AddOrUpdate(string archiveFilePath, string filePathToAdd)
		{
			VerifyInstalled();
			if (!File.Exists(archiveFilePath)) throw new FileNotFoundException(archiveFilePath);
			if (!File.Exists(filePathToAdd)) throw new FileNotFoundException(filePathToAdd);
			string folder = Path.GetDirectoryName(archiveFilePath)!, name = Path.GetFileName(archiveFilePath);
			List<string> args = ["a", name];

			args.Add(filePathToAdd);
			var r = await Runner.Execute(ExecutablePath, folder, args.ToArray());
			return r;
		}

		public static void CreateEmptyZip(string filePath)
		{
			byte[] empty = { 0x50, 0x4B, 0x05, 0x06, 0x00, 0x00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
			File.WriteAllBytes(filePath, empty);
		}

		public static void CreateEmpty7Z(string filePath)
		{
			byte[] empty = { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C, 0x00, 0x04, 0x8D, 0x9B, 0xD5, 0x0F, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
			File.WriteAllBytes(filePath, empty);
		}

		private static readonly Regex _rxHeader = new Regex(@"(\d+)\sfolders, (\d+) files, (\d+) bytes \((\d+) MiB\)", RegexOptions.Compiled);
		private static readonly Regex _rxSoloPct = new Regex(@"(\d+)%\s\+\s(\d+)\.7z", RegexOptions.Compiled);
		private static readonly Regex _rxFullPct = new Regex(@"(\d+)%\s(\d+)\s\+\s(.*)", RegexOptions.Compiled);

		#region Progress

		public record struct Progress(int TotalFolderCount, int TotalFileCount, long TotalBytes, int TotalMegabytes, double PercentComplete,
			int CurrentFileIndex, string CurrentFileName)
		{
			public static readonly Progress Empty = new Progress(0, 0, 0, 0, 0.0, 0, string.Empty);
			public bool IsEmpty => TotalFolderCount == 0 && TotalFileCount == 0 && TotalBytes == 0;
			public bool HasFileName => !string.IsNullOrEmpty(CurrentFileName);
		}

		private class InputProcessor
		{
			private Progress Progress { get; set; } = Progress.Empty;
			private Action<Progress> Action { get; init; } = Actions<Progress>.Empty;

			internal InputProcessor(Action<Progress>? action)
			{
				if (action != null) Action = action;
			}

			internal void Process(string line)
			{
				//System.Diagnostics.Debug.WriteLine(line);
				if (string.IsNullOrEmpty(line?.Trim())) return;
				if (Progress.IsEmpty)
				{
					Match m = _rxHeader.Match(line);
					if (m.Success)
					{
						Progress = new Progress
						{
							TotalFolderCount = int.Parse(m.Groups[1].Value),
							TotalFileCount = int.Parse(m.Groups[2].Value),
							TotalBytes = long.Parse(m.Groups[3].Value),
							TotalMegabytes = int.Parse(m.Groups[4].Value)
						};
						Action?.Invoke(Progress);
					}
				}
				else
				{
					if (!Progress.HasFileName)
					{
						Match m = _rxSoloPct.Match(line);
						if (m.Success)
						{
							double pcnt = double.Parse(m.Groups[1].Value);
							Progress = Progress with { PercentComplete = pcnt };
							Action?.Invoke(Progress);
						}
						else goto parseFull;
					}
					parseFull:
					{
						Match m = _rxFullPct.Match(line);
						if (m.Success)
						{
							Progress = Progress with
							{
								PercentComplete = double.Parse(m.Groups[1].Value),
								CurrentFileIndex = int.Parse(m.Groups[2].Value),
								CurrentFileName = m.Groups[3].Value
							};
							Action?.Invoke(Progress);
						}
					}
				}

			}
		}

		#endregion

		#region ArchiveContents

		public class ArchiveContents
		{
			public static ArchiveContents FromOutput(IEnumerable<string> output)
			{
				ArchiveContents r = new ArchiveContents();
				Dictionary<string, FolderContent> folders = new();
				bool blocksFound = false, pathFound = false;
				int[]? columnLengths = null;
				string[] parts = new string[5];
				foreach(string line in output)
				{
					if (line == null) continue;
					if (columnLengths != null) goto processLine;
					if (!pathFound && !line.StartsWith("Path =")) continue;
					if (!blocksFound)
					{
						int eqPos = line.IndexOf('=');
						if (eqPos < 0)
						{
							blocksFound = true; // zip file?
							continue;
						}
						string val = line.Substring(eqPos + 1).Trim();
						switch(line.Substring(0, 4))
						{
							case "Path": r.Path = val; pathFound = true; break;
							case "Type": r.Type = val; break;
							case "Phys": r.PhysicalSize = long.Parse(val); break;
							case "Head": r.HeadersSize = int.Parse(val); break;
							case "Meth": r.Method = val; break;
							case "Soli": r.Solid = val; break;
							case "Bloc": blocksFound = true; r.Blocks = int.Parse(val); break;
						}
						continue;
					}
					if (columnLengths == null && !line.StartsWith("--")) continue;
					// ------------------- ----- ------------ ------------  ------------------------
					columnLengths = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Length).ToArray();
					continue;
					processLine:
					if (line.StartsWith('-')) continue;
					int lastLen = 0, n = 0;
					foreach(int len in columnLengths)
					{
						switch(n)
						{
							case 0: parts[n] = line.Substring(0, len).Trim(); break;
							case 4: parts[n] = line.Substring(lastLen + n).Trim(); break;
							default: parts[n] = line.Substring(lastLen + n, len).Trim(); break;
						}
						lastLen += len;
						n++;
					}
					if (parts[2] == "0" && parts[3] == "0")	// Folder
					{
						string[] nameParts = parts[4].Split('\\');
						if (nameParts.Length == 1)
						{
							folders.Add(nameParts[0], new FolderContent(parts));
						} else
						{
							folders[nameParts[0]].AddSubfolder(parts, nameParts);
						}
						continue;
					}
					if (string.IsNullOrEmpty(parts[1]))
					{
						// last line - no attrs
						r.ContentSummary = new SummaryContent(parts);
					} else
					{
						// Building Loops\Attention Span Project\Ableton Project Info\AProject.ico
						string[] nameParts = parts[4].Split('\\');
						if (nameParts.Length == 1) r._contents.Add(new FileContent(parts)); else
						{
							FolderContent folder = folders[nameParts[0]];
							for(int i=1;i<nameParts.Length-1;i++)
							{
								folder = folder[nameParts[i]]!;
							}
							r._contents.Add(new FileContent(parts, folder));
						}
					}
				}
				r._folders.AddRange(folders.Values);
				return r;
			}

			private List<FileContent> _contents = new();
			private List<FolderContent> _folders = new();
			private ArchiveContents() { }

			public string Path { get; private set; } = string.Empty;
			public string ArchiveFilePath { get; private init; } = string.Empty;
			public string Type { get; private set; } = string.Empty;
			public long PhysicalSize { get; private set; }
			public int HeadersSize { get; private set; }
			public string Method { get; private set; } = string.Empty;
			public string Solid { get; private set; } = string.Empty;
			public int Blocks { get; private set; }
			public int TotalFileCount => _contents.Count;
			public IReadOnlyList<FileContent> Contents => _contents.AsReadOnly();
			public IReadOnlyList<FolderContent> Folders => _folders.AsReadOnly();
			public SummaryContent? ContentSummary { get; private set; }

			public abstract class Content
			{
				protected Content(string[] parts)
				{
					string sdt = parts[0];  // 2024-04-22 20:21:43
					DateTime = DateTime.ParseExact(sdt, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
					if (long.TryParse(parts[2], out long l)) Size = l;
					if (long.TryParse(parts[3], out l)) CompressedSize = l;
				}

				public DateTime DateTime { get; protected set; }
				public virtual long Size { get; protected set; }
				public long CompressedSize { get; private set; }
				public double Compression => (double)CompressedSize / (double)Size;
			}

			public class FolderContent : Content
			{
				private static readonly List<FolderContent> _emptySubs = new();
				private List<FolderContent>? _subfolders;
				private List<FileContent> _files = new();
				internal FolderContent(string[] parts): base(parts)
				{
					Name = parts[4];
				}

				private FolderContent(FolderContent parent, string name, string[] parts): base(parts)
				{
					ParentFolder = parent;
					Name = name;
				}

				public string Name { get; private init; }
				public FolderContent? ParentFolder { get; private init; }
				public bool HasSubfolders => _subfolders != null;
				public IReadOnlyList<FolderContent> Subfolders => (_subfolders == null) ? _emptySubs.AsReadOnly() : _subfolders.AsReadOnly();
				public FolderContent? this[string name] => Subfolders.FirstOrDefault(f => f.Name ==  name);
				public IReadOnlyList<FileContent> Files => _files.AsReadOnly();
				internal void AddSubfolder(string[] parts, string[] nameParts)
				{
					AddSubfolder(parts, nameParts, 0);
				}

				private void AddSubfolder(string[] parts, string[] nameParts, int startAt)
				{
					if (!(nameParts[startAt] == Name)) throw new InvalidOperationException();
					string name = nameParts[startAt + 1];
					FolderContent? sub = (_subfolders == null) ? null : _subfolders.FirstOrDefault(f => f.Name == name);
					if (sub == null)
					{
						if (_subfolders == null) _subfolders = new();
						_subfolders.Add(new FolderContent(this, name, parts));
					}
					else sub.AddSubfolder(parts, nameParts, startAt + 1);
				}

				internal void AddFile(FileContent file) => _files.Add(file);
				

				public override string ToString() => Name;
			}

			public class SummaryContent : Content
			{
				internal SummaryContent(string[] parts): base(parts)
				{
					string[] sizes = parts[4].Split(',');
					int spos = sizes[0].IndexOf(' ');
					FileCount = int.Parse(sizes[0].Substring(0, spos));
					if (sizes.Length == 2)
					{
						spos = sizes[1].LastIndexOf(' ');
						FolderCount = int.Parse(sizes[1].Substring(0, spos));
					}
				}

				public int FileCount { get; private init; }
				public int FolderCount { get; private init; }
			}

			public class FileContent : Content
			{
				internal FileContent(string[] parts): base(parts)
				{
					Attributes = parts[1];
					FileName = parts[4];
				}

				internal FileContent(string[] parts, FolderContent folder): this(parts)
				{
					Folder = folder;
					Folder.AddFile(this);
				}

				public string FileName { get; private init; }
				public string Attributes { get; private init; }
				public FolderContent? Folder { get; private init; }

				public override string ToString() => FileName;
			}
		}

		#endregion
	}
}
