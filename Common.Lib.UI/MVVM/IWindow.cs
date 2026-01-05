using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Common.Lib.UI.MVVM
{
	/// <summary>
	/// IWindow represents core Window properties/behaviors accessible to a ViewModel
	/// </summary>
	public interface IWindow
	{
		bool IsActive { get; }
		bool Activate();
		void Close();
		void Hide();
		void Show();
		event CancelEventHandler? Closing;
		event EventHandler? Closed;
		event KeyEventHandler? KeyDown;
		object DataContext { get; set; }
		double Left { get; set; }
		double Top { get; set; }
		public Window? Owner { get; set; }
		bool? DialogResult { get; set; }
	}
}
