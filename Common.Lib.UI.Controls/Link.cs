using Common.Lib.UI.Adorners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Common.Lib.UI.Controls
{
	/// <summary>
	/// A more generalized (hyper)Link containing custom content
	/// </summary>
	public class Link : ContentControl
	{
		internal static readonly Link Default = new();
		public enum LinkUnderlineBehavior
		{
			AlwaysUnderline,
			NeverUnderline,
			UnderlineOnHover
		}

		#region static interface

		private static UnderlineAdorner _defaultUnderline = new UnderlineAdorner(DefaultControls.ContentControl);

		static Link()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Link), new FrameworkPropertyMetadata(typeof(Link)));
		}

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand),
			typeof(Link), new PropertyMetadata(null));

		public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object),
			typeof(Link), new PropertyMetadata(null));

		public static readonly DependencyProperty UseClickPreviewProperty = DependencyProperty.Register("UseClickPreview", typeof(bool),
			typeof(Link), new PropertyMetadata(false));

		public static readonly DependencyProperty UnderlineBehaviorProperty = DependencyProperty.Register("UnderlineBehavior", typeof(LinkUnderlineBehavior),
			typeof(Link), new PropertyMetadata(LinkUnderlineBehavior.AlwaysUnderline, HandleUnderlineBehaviorChanged));

		public static readonly DependencyProperty UnderlineBrushProperty = DependencyProperty.Register("UnderlineBrush", typeof(Brush),
			typeof(Link), new PropertyMetadata(Brushes.Black));

		public static readonly DependencyProperty DisabledBackgroundProperty = DependencyProperty.Register("DisabledBackground", typeof(Brush),
			typeof(Link), new PropertyMetadata(Brushes.LightSlateGray));

		public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Link));

		private static void HandleUnderlineBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Link)d).ApplyUnderlineBehavior();
		}

		#endregion

		#region Dependency Properties / Events

		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public bool UseClickPreview
		{
			get => (bool)GetValue(UseClickPreviewProperty);
			set => SetValue(UseClickPreviewProperty, value);
		}

		public LinkUnderlineBehavior UnderlineBehavior
		{
			get => (LinkUnderlineBehavior)GetValue(UnderlineBehaviorProperty);
			set => SetValue(UnderlineBehaviorProperty, value);
		}

		public Brush UnderlineBrush
		{
			get => (Brush)GetValue(UnderlineBrushProperty);
			set => SetValue(UnderlineBrushProperty, value);
		}

		public Brush DisabledBackground
		{
			get => (Brush)GetValue(DisabledBackgroundProperty);
			set => SetValue(DisabledBackgroundProperty, value);
		}

		public event RoutedEventHandler Click
		{
			add { AddHandler(ClickEvent, value); }
			remove { RemoveHandler(ClickEvent, value); }
		}

		#endregion

		private ContentPresenter _content = DefaultControls.ContentPresenter;
		private Border _outerBorder = DefaultControls.Border;
		private UnderlineAdorner _underline = _defaultUnderline;

		private bool IsTemplateApplied { get; set; }
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_content = (ContentPresenter)GetTemplateChild("content");
			_outerBorder = (Border)GetTemplateChild("outerBorder");
			_underline = new UnderlineAdorner(_content, UnderlineBrush, false);
			AdornerLayer.GetAdornerLayer(this).Add(_underline);   // will throw if AdornerLayer not present
			IsTemplateApplied = true;
			ApplyCommandParameter();
		}

		private void ApplyUnderlineBehavior()
		{
			if (!IsTemplateApplied) return;
			switch (UnderlineBehavior)
			{
				case LinkUnderlineBehavior.UnderlineOnHover:
				case LinkUnderlineBehavior.NeverUnderline: _underline.IsActive = false; break;
				default: _underline.IsActive = true; break;
			}
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);
			if (IsEnabled)
			{
				if (UnderlineBehavior == LinkUnderlineBehavior.UnderlineOnHover) _underline.IsActive = true;
				Cursor = Cursors.Hand;
			}
			else Cursor = Cursors.Arrow;
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			if (UnderlineBehavior == LinkUnderlineBehavior.UnderlineOnHover) _underline.IsActive = false;
			Cursor = Cursors.Arrow;
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

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			switch (e.Property.Name)
			{
				case nameof(IsEnabled): _underline.IsActive = IsEnabled; break;
				case nameof(Command): ApplyCommand((ICommand)e.OldValue, (ICommand)e.NewValue); break;
				case nameof(CommandParameter): ApplyCommandParameter(); break;
				case nameof(UnderlineBrush): if (IsTemplateApplied) _underline.LineBrush = UnderlineBrush; break;
			}
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
			IsEnabled = Command == null || Command.CanExecute(CommandParameter);
			if (IsEnabled) _outerBorder.Background = Background; else _outerBorder.Background = DisabledBackground == null ? Background : DisabledBackground;
			ApplyUnderlineBehavior();
		}
	}
}
