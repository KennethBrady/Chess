using Chess.Lib.Pgn;
using Common.Lib.UI;
using System.Windows;
using System.Windows.Controls;

namespace Chess.Lib.UI.Pgn
{
	public class PgnTagEditor : Control
	{
		static PgnTagEditor()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PgnTagEditor), new FrameworkPropertyMetadata(typeof(PgnTagEditor)));
		}

		private TextBox NewTag { get; set; } = DefaultControls.TextBox;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			NewTag = (TextBox)GetTemplateChild("newTag");
			if (DataContext is TagEditorModel tem) tem.FocusNewTagTextBox = () => NewTag.Focus();
		}
	}

	internal class TagEditorTemplateSelector : DataTemplateSelector
	{
		public DataTemplate StringTemplate { get; set; } = DefaultControls.DataTemplate;

		public DataTemplate ReqStringTemplate { get; set; } = DefaultControls.DataTemplate;
		public DataTemplate DateTemplate { get; set; } = DefaultControls.DataTemplate;
		public DataTemplate ResultTemplate { get; set; } = DefaultControls.DataTemplate;

		public override DataTemplate? SelectTemplate(object item, DependencyObject container)
		{
			if (item is TagEditorModel.TagModel m)
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
