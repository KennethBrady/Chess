using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CommonApps.Lib.Console
{
	public readonly record struct ConsoleResult(int ExitCode, ImmutableList<string> Output, ImmutableList<string> Errors)
	{
		public static readonly ConsoleResult Empty = new ConsoleResult(int.MinValue, [], []);
		public bool Succeeded => ExitCode == 0;
		public string LineOutput => string.Join(Environment.NewLine, Output);

		public string ErrorString => string.Join(';', Errors);
	}

	[Flags]
	public enum OutputType { StandardOut = 0x01, StandardError = 0x02, Both = StandardOut | StandardError };

	public static class Runner
	{
		public static Process StartCmdLine(string folder)
		{
			Process p = new Process();
			p.StartInfo.FileName = "cmd.exe";
			p.StartInfo.WorkingDirectory = folder;
			p.StartInfo.ArgumentList.Add("/K");
			p.Start();
			return p;
		}

		public static Task<ConsoleResult> ExecuteWithFeedback(string exePath, string workingDirectory, IEnumerable<string> arguments, int processPriority, Action<OutputType, string>? feedback)
		{
			if (!File.Exists(exePath)) throw new ArgumentException($"File not found: {exePath}");
			if (!Directory.Exists(workingDirectory)) throw new ArgumentException($"Directory not found: {workingDirectory}");
			if (exePath.Contains(' ') && !exePath.StartsWith('"')) exePath = $"\"{exePath}\"";
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = exePath,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = workingDirectory,
				UseShellExecute = false
			};
			if (arguments != null) foreach (string arg in arguments) psi.ArgumentList.Add(arg);
			ConsoleResult run()
			{
				Process p = new Process { StartInfo = psi };
				List<string> result = new List<string>(), errors = new List<string>();
				void append(OutputType type, string? data)
				{
					if (string.IsNullOrEmpty(data)) return;
					feedback?.Invoke(type, data);
					List<string> s = (type == OutputType.StandardOut) ? result : errors;
					s.Add(data);
				}
				p.OutputDataReceived += (o, e) =>
				{
					append(OutputType.StandardOut, e.Data);
				};
				p.ErrorDataReceived += (o, e) =>
				{
					append(OutputType.StandardError, e.Data);
				};
				p.Start();
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();
				p.WaitForExit();
				return new ConsoleResult(p.ExitCode, result.ToImmutableList(), errors.ToImmutableList());
			}
			return Task<ConsoleResult>.Factory.StartNew(run);
		}

		public static Task<ConsoleResult> Execute(string exePath, string workingDirectory, params string[] arguments) =>
			ExecuteWithFeedback(exePath, workingDirectory, arguments, 0, null);

		public static bool TryKillProcess(int procId)
		{
			try
			{
				Process p = Process.GetProcessById(procId);
				p.Kill(true);
				return true;
			}
			catch
			{
				return false; 
			}
		}

		public static async Task<string> ExecuteAndReturnOutput(string exePath, string workingDirectory, int processPriority,
	params string[] parameters)
		{
			var result = await Execute(exePath,workingDirectory, parameters);
			return result.LineOutput;
		}
	

	}
}
