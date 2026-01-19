using Common.Lib.Contracts;
using Common.Lib.UI;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.Settings;
using System.Configuration;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Common.Lib.UI.Windows
{
	/// <summary>
	/// Represents a window with customizable chrome, menu and dialogs
	/// </summary>
	public class AppWindow : Window, IAppWindow
	{
		private static readonly DialogLayer DefaultDialogLayer = new();
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

		public static readonly DependencyProperty ModalBackgroundProperty = DialogLayer.ModalBackgroundProperty.AddOwner(typeof(AppWindow));

		public static readonly DependencyProperty AppSettingsProperty = DependencyProperty.Register("AppSettings", typeof(ApplicationSettingsBase),
			typeof(AppWindow), new PropertyMetadata(new DefaultAppSettings()));

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

		public Brush ModalBackground
		{
			get => (Brush)GetValue(ModalBackgroundProperty);
			set => SetValue(ModalBackgroundProperty, value);
		}

		public ApplicationSettingsBase AppSettings
		{
			get => (ApplicationSettingsBase)GetValue(AppSettingsProperty);
			set => SetValue(AppSettingsProperty, value);
		}

		#endregion

		public AppWindow()
		{
			Loaded += (o, e) => OnLoaded();
			SizeChanged += (o, e) => OnSizeChanged(e.PreviousSize, e.NewSize);
		}

		protected virtual void OnLoaded() { }

		protected virtual void OnSizeChanged(Size oldSize, Size newSize) { }

		protected TitleBar TitleBar { get; private set; } = TitleBar.Default;

		protected bool IsTemplateApplied { get; private set; }

		private Grid Grid { get; set; } = DefaultControls.Grid;
		private DialogLayer DialogLayer { get; set; } = DefaultDialogLayer;
		private ContentPresenter ContentPresenter { get; set; } = DefaultControls.ContentPresenter;

		public override void OnApplyTemplate()
		{
			IsTemplateApplied = true;
			base.OnApplyTemplate();
			Grid = (Grid)GetTemplateChild("grid");
			TitleBar = (TitleBar)GetTemplateChild("titleBar");
			TitleBar.MouseDoubleClick += (o, e) => TitleBarDoubleClick(e.GetPosition(TitleBar));
			DialogLayer = (DialogLayer)GetTemplateChild("dialogLayer");
			ContentPresenter = (ContentPresenter)GetTemplateChild("content");
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

		protected override void OnClosed(EventArgs e)
		{
			if (!AppSettings.IsDefault) AppSettings.Save();
			base.OnClosed(e);
		}

		protected IDisposable GoModal()
		{
			DialogLayer.IsHitTestVisible = true;
			return new ActionDisposer(() => DialogLayer.IsHitTestVisible = false);
		}

		async Task<IDialogResult<T>> IAppWindow.ShowDialog<T>(IDialogModel<T> dialogContext)
		{
			if (Dialogs == null || Dialogs.Count == 0) return new DialogResultFailure<T>($"No dialogs have been registered.");
			if (dialogContext is not IDialogModelEx<T> ex) return new DialogResultFailure<T>($"{nameof(dialogContext)} is not a DialogModel.");
			Type modelType = dialogContext.GetType();
			DialogDef dd = Dialogs.FirstOrDefault(d => d.ModelType == modelType);
			if (!dd.IsViewTypeValid) return new DialogResultFailure<T>($"Dialog with model type {modelType.Name} is not registered.");
			if (!dd.IsModelTypeValid) return new DialogResultFailure<T>($"Type {dd.DialogType.Name} is not derived from {typeof(DialogView).Name}.");

			// Create dialog:
			ConstructorInfo? c = dd.DialogType.GetConstructor(Type.EmptyTypes);
			if (c == null) return new DialogResultFailure<T>("DialogView must have an empty public constructor.");
			DialogView? dialog = (DialogView)c.Invoke(null);
			if (dialog == null) return new DialogResultFailure<T>("Constructor returned null.");
			if (dialog.Style == null && DialogStyle != null) dialog.Style = DialogStyle;
			SettingsManager.ApplySettings(dialogContext, AppSettings, dd.SettingsKey);
			ex.Closing += (result) =>
			{
				if (result.Accepted) SettingsManager.ExtractAndSaveSettings(dialogContext, AppSettings, dd.SettingsKey);
			};
			return await DialogLayer.PushDialog(dialog, ex);	
		}
	}
}
