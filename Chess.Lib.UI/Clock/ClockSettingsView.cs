using System.Windows;
using System.Windows.Controls;

namespace Chess.Lib.UI.Clock
{
	public class ClockSettingsView : Control
	{
		static ClockSettingsView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ClockSettingsView), new FrameworkPropertyMetadata(typeof(ClockSettingsView)));
		}
	}
}
