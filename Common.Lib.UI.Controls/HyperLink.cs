using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Common.Lib.UI.Controls
{
	/// <summary>
	/// A common HyperLink containing Text
	/// </summary>
	public class HyperLink : Control
	{
		public enum HyperLinkUnderlineBehavior
		{
			AlwaysUnderline,
			NeverUnderline,
			UnderlineOnHover
		}

		#region static interface

		static HyperLink()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(HyperLink), new FrameworkPropertyMetadata(typeof(HyperLink)));
		}

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(HyperLink),
			new PropertyMetadata(null, HandleCommandPropertyChanged));

		public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object),
			typeof(HyperLink), new PropertyMetadata(null, HandleCommandPropertyChanged));

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HyperLink));

		public static readonly DependencyProperty UnderlineBehaviorProperty = DependencyProperty.Register("UnderlineBehavior", typeof(HyperLinkUnderlineBehavior),
			typeof(HyperLink), new PropertyMetadata(HyperLinkUnderlineBehavior.AlwaysUnderline, HandleUnderlineBehaviorChanged));

		public static readonly DependencyProperty TextTrimmingProperty = TextBlock.TextTrimmingProperty.AddOwner(typeof(HyperLink),
			new PropertyMetadata(TextTrimming.None));

		public static readonly DependencyProperty TextWrappingProperty = TextBlock.TextWrappingProperty.AddOwner(typeof(HyperLink),
			new PropertyMetadata(TextWrapping.Wrap));

		public static readonly DependencyProperty UseClickPreviewProperty = DependencyProperty.Register("UseClickPreview", typeof(bool),
			typeof(HyperLink), new PropertyMetadata(false));

		public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HyperLink));

		private static void HandleUnderlineBehaviorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			HyperLink hl = (HyperLink)o;
			hl.ApplyUnderlineBehavior();
		}

		private static void HandleCommandPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			HyperLink hl = (HyperLink)o;
			if (hl.Command == null) return;
			switch (e.Property.Name)
			{
				case "Command": hl.ApplyCommand((ICommand)e.OldValue, (ICommand)e.NewValue); break;
				case "CommandParameter": hl.ApplyCommandParameter(); break;
			}
		}

		#endregion

		#region DependencyProperties

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public HyperLinkUnderlineBehavior UnderlineBehavior
		{
			get => (HyperLinkUnderlineBehavior)GetValue(UnderlineBehaviorProperty);
			set => SetValue(UnderlineBehaviorProperty, value);
		}

		public TextTrimming TextTrimming
		{
			get => (TextTrimming)GetValue(TextTrimmingProperty);
			set => SetValue(TextTrimmingProperty, value);
		}

		public TextWrapping TextWrapping
		{
			get => (TextWrapping)GetValue(TextWrappingProperty);
			set => SetValue(TextWrappingProperty, value);
		}

		public bool UseClickPreview
		{
			get => (bool)GetValue(UseClickPreviewProperty);
			set => SetValue(UseClickPreviewProperty, value);
		}

		public event RoutedEventHandler Click
		{
			add { AddHandler(ClickEvent, value); }
			remove { RemoveHandler(ClickEvent, value); }
		}

		#endregion

		private TextBlock _hlink = DefaultControls.TextBlock;
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_hlink = (TextBlock)GetTemplateChild("hlink");
			ApplyUnderlineBehavior();
		}

		private void ApplyUnderlineBehavior()
		{
			if (_hlink == null) return;
			if (!IsEnabled) _hlink.TextDecorations = null;
			else
			{
				switch (UnderlineBehavior)
				{
					case HyperLinkUnderlineBehavior.AlwaysUnderline: _hlink.TextDecorations = TextDecorations.Underline; break;
					case HyperLinkUnderlineBehavior.UnderlineOnHover:
					case HyperLinkUnderlineBehavior.NeverUnderline: _hlink.TextDecorations = null; break;
				}
			}
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);
			if (!IsEnabled) _hlink.TextDecorations = null;
			else
			if (UnderlineBehavior == HyperLinkUnderlineBehavior.UnderlineOnHover) _hlink.TextDecorations = TextDecorations.Underline;
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			if (UnderlineBehavior == HyperLinkUnderlineBehavior.UnderlineOnHover) _hlink.TextDecorations = null;
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);
			if (UseClickPreview) ApplyCommand(e);
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			if (!UseClickPreview) ApplyCommand(e);
		}

		private void ApplyCommand(MouseButtonEventArgs e)
		{
			if (Command != null && Command.CanExecute(CommandParameter))
			{
				Command.Execute(CommandParameter);
				e.Handled = true;
			}
			RaiseEvent(new RoutedEventArgs(ClickEvent, this));
		}

		private void ApplyCommand(ICommand oldValue, ICommand newValue)
		{
			if (oldValue != null) oldValue.CanExecuteChanged -= HandleCommandCanExecuteChanged;
			if (newValue != null) newValue.CanExecuteChanged += HandleCommandCanExecuteChanged;
		}

		private void HandleCommandCanExecuteChanged(object? sender, EventArgs e)
		{
			ApplyCommandParameter();
		}

		private void ApplyCommandParameter()
		{
			IsEnabled = Command != null && Command.CanExecute(CommandParameter);
			ApplyUnderlineBehavior();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property.Name == "IsEnabled") ApplyUnderlineBehavior();
		}

	}
}
