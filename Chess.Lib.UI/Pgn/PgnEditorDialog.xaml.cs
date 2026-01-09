using Chess.Lib.Pgn;
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

		protected override void OnDataContextChanged(object? oldValue, object? newValue)
		{
			base.OnDataContextChanged(oldValue, newValue);
			if (newValue is PgnEditorModel m) m.FocusNewTagTextBox = () => newTag.Focus();
		}
	}

	internal class PgnEditorTemplateSelector : DataTemplateSelector
	{
		public DataTemplate StringTemplate { get; set; } = DefaultControls.DataTemplate;

		public DataTemplate ReqStringTemplate { get; set; } = DefaultControls.DataTemplate;
		public DataTemplate DateTemplate { get; set; } = DefaultControls.DataTemplate;
		public DataTemplate ResultTemplate { get; set; } = DefaultControls.DataTemplate;

		public override DataTemplate? SelectTemplate(object item, DependencyObject container)
		{
			if (item is PgnEditorModel.TagModel m)
			{
				if (m.Tag == PgnTags.Date) return DateTemplate;
				if (m.Tag == PgnTags.Result) return ResultTemplate;
				if (m.IsRequired) return ReqStringTemplate;
				return StringTemplate;
			}
			return null;
		}
	}

}
