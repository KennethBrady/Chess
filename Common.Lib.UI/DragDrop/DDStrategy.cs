using Common.Lib.Extensions;
using System.Windows;

namespace	Common.Lib.UI.DragDrop
{
	/// <summary>
	/// Provides a simplified and MVVM-compatible approach to handling Drag/Drop
	/// </summary>
	public interface IDDStrategy
	{
		bool Accept(IDataObject data);
		IEnumerable<string> AcceptedFormats { get; }
		DragDropEffects AllowedEffects => DragDropEffects.Copy;
		void HandleDrop(object data);
	}

	public abstract class DDStrategy : IDDStrategy
	{
		public abstract bool Accept(IDataObject data);
		public abstract IEnumerable<string> AcceptedFormats { get; }
		public abstract DragDropEffects AllowedEffects { get; }
		public abstract void HandleDrop(object data);
	}

	public record struct EmptyStrategy : IDDStrategy
	{
		public static readonly EmptyStrategy Default = new();

		IEnumerable<string> IDDStrategy.AcceptedFormats => Enumerable.Empty<string>();

		bool IDDStrategy.Accept(IDataObject data) => false;		

		void IDDStrategy.HandleDrop(object data) { }
		
	}

	public class FileDDStrategy : IDDStrategy
	{
		public FileDDStrategy(Action<IEnumerable<string>> handleDrop) 
		{ 
			DropHandler = handleDrop;
		}

		public bool Accept(IDataObject data) => data.GetDataPresent(DataFormats.FileDrop) || data.GetDataPresent(DataFormats.Text);

		public IEnumerable<string> AcceptedFormats => [DataFormats.FileDrop, DataFormats.Text];

		private Action<IEnumerable<string>> DropHandler { get; init; }

		void IDDStrategy.HandleDrop(object data)
		{
			if (data is string[] paths) DropHandler(paths);
			if (data is string path) DropHandler(path.Yield());
		}
	}
}
