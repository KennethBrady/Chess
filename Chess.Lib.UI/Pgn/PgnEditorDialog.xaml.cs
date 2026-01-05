using Chess.Lib.Pgn.Parsing;
using Common.Lib.UI;
using Common.Lib.UI.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace Chess.Lib.UI.Pgn
{
	/// <summary>
	/// Interaction logic for PgnEditorDialog2.xaml
	/// </summary>
	public partial class PgnEditorDialog : DialogView
	{
		public PgnEditorDialog()
		{
			InitializeComponent();
		}
	}

	internal class PgnEditorTemplateSelector : DataTemplateSelector
	{
		public DataTemplate StringTemplate { get; set; } = DefaultControls.DataTemplate;
		public DataTemplate DateTemplate { get; set; } = DefaultControls.DataTemplate;
		public DataTemplate ResultTemplate { get; set; } = DefaultControls.DataTemplate;

		public override DataTemplate? SelectTemplate(object item, DependencyObject container)
		{
			if (item is PgnEditorModel.TagModel m)
			{
				if (m.Tag == PgnSourceParser.DateTag) return DateTemplate;
				if (m.Tag == PgnSourceParser.ResultTag) return ResultTemplate;
				return StringTemplate;
			}
			return null;
		}
	}

}
