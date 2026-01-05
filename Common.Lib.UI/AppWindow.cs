using Common.Lib.Contracts;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.MVVM;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Common.Lib.UI
{
	public interface IAppWindow : IWindow
	{
		Task<IDialogResult<T>> ShowDialog<T>(IDialogModel<T> dialogContext);
	}

	public record struct DialogDef(Type DialogType, Type ModelType);

	/// <summary>
	/// Represents a window with customizable chrome, menu and dialogs
	/// </summary>
	public class AppWindow : Window, IAppWindow
	{
		#region Static Interface
		static AppWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(AppWindow), new FrameworkPropertyMetadata(typeof(AppWindow)));
		}

		public static readonly DependencyProperty TitleBarStyleProperty = DependencyProperty.Register("TitleBarStyle", typeof(Style),
			typeof(AppWindow), new PropertyMetadata(null, null, CoerceTitleBarStyle));

		public static readonly DependencyProperty DialogsProperty = DependencyProperty.Register("Dialogs", typeof(List<DialogDef>),
			typeof(AppWindow), new PropertyMetadata(new List<DialogDef>()));

		public static readonly DependencyProperty MenuProperty = DependencyProperty.Register("Menu", typeof(Menu),
			typeof(AppWindow), new PropertyMetadata(null));

		public static readonly DependencyProperty DialogStyleProperty = DependencyProperty.Register("DialogStyle", typeof(Style),
			typeof(AppWindow), new PropertyMetadata(null, null, CoerceDialogStyle));

		private static object? CoerceTitleBarStyle(DependencyObject o, object? value) => (value is Style s && s.TargetType == typeof(TitleBar)) ? s : null;

		private static object? CoerceDialogStyle(DependencyObject o, object? value) => (value is Style s && s.TargetType == typeof(DialogView)) ? s : null;

		#endregion

		#region Dependency Properties

		public Style? TitleBarStyle
		{
			get { return (Style)GetValue(TitleBarStyleProperty); }
			set { SetValue(TitleBarStyleProperty, value); }
		}

		public List<DialogDef>? Dialogs
		{
			get => (List<DialogDef>)GetValue(DialogsProperty);
			set => SetValue(DialogsProperty, value);
		}

		public Menu Menu
		{
			get => (Menu)GetValue(MenuProperty);
			set => SetValue(MenuProperty, value);
		}

		public Style DialogStyle
		{
			get => (Style)GetValue(DialogStyleProperty);
			set => SetValue(DialogStyleProperty, value);
		}

		#endregion

		public AppWindow()
		{
			Loaded += (o, e) => OnLoaded();
			SizeChanged += (o, e) => OnSizeChanged(e.PreviousSize, e.NewSize);
			DialogLayer = new DialogLayer();
		}

		protected virtual void OnLoaded() { }

		protected virtual void OnSizeChanged(Size oldSize, Size newSize) { }

		protected TitleBar TitleBar { get; private set; } = TitleBar.Default;

		protected bool IsTemplateApplied { get; private set; }

		private Grid Grid { get; set; } = DefaultControls.Grid;
		private DialogLayer DialogLayer { get; init; }
		private ContentPresenter ContentPresenter { get; set; } = DefaultControls.ContentPresenter;
		public override void OnApplyTemplate()
		{
			IsTemplateApplied = true;
			base.OnApplyTemplate();
			Grid = (Grid)GetTemplateChild("grid");
			TitleBar = (TitleBar)GetTemplateChild("titleBar");
			TitleBar.MouseDoubleClick += TitleBar_MouseDoubleClick;
			ContentPresenter = (ContentPresenter)GetTemplateChild("content");
			Grid.SetRow(DialogLayer, 1);
			Grid.Children.Add(DialogLayer);
		}

		protected virtual void TitleBarDoubleClick(Point position)
		{
			switch (WindowState)
			{
				case WindowState.Normal:
					if (TitleBar.ButtonTypes.HasFlag(TitleBar.WindowButtonType.Restore)) WindowState = WindowState.Maximized;
					break;
				case WindowState.Maximized:
					if (TitleBar.ButtonTypes.HasFlag(TitleBar.WindowButtonType.Restore)) WindowState = WindowState.Normal;
					break;
			}
		}

		private void TitleBar_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
			TitleBarDoubleClick(e.GetPosition(TitleBar));

		protected IDisposable GoModal()
		{
			TitleBar.IsEnabled = false;
			ContentPresenter.IsEnabled = false;
			return new ActionDisposer(() =>
			{
				TitleBar.IsEnabled = true;
				ContentPresenter.IsEnabled = true;
			});
		}

		private static readonly Type DialogViewType = typeof(DialogView);
		async Task<IDialogResult<T>> IAppWindow.ShowDialog<T>(IDialogModel<T> dialogContext)
		{
			if (dialogContext is not IDialogModelEx<T> ex) return new DialogResultFailure<T>($"{nameof(dialogContext)} is not a DialogModel.");
			Type modelType = dialogContext.GetType();
			DialogDef? dd= Dialogs?.FirstOrDefault(d => d.ModelType == modelType);
			if (!dd.HasValue) return new DialogResultFailure<T>($"Dialog with model type {modelType.Name} is not registered.");
			Type dialogType = dd.Value.DialogType;
			if (!dialogType.IsAssignableTo(DialogViewType)) return new DialogResultFailure<T>($"Type {dialogType.Name} is not derived from {DialogViewType.Name}.");
			ConstructorInfo? c = dialogType.GetConstructor(Type.EmptyTypes);
			if (c == null) return new DialogResultFailure<T>("DialogView must have an empty public constructor.");
			DialogView? dialog = (DialogView)c.Invoke(null);
			if (dialog == null) return new DialogResultFailure<T>("Constructor returned null.");
			if (dialog.Style == null && DialogStyle != null) dialog.Style = DialogStyle;
			using IDisposable modal = dialog.IsModal ? GoModal() : ActionDisposer.NoAction;
			return await DialogLayer.PushDialog(dialog, ex);
		}
	}
}
