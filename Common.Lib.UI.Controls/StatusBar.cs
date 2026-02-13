using Common.Lib.UI.Controls.Extensions;
using Common.Lib.UI.Controls.Models;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Common.Lib.UI.Controls
{
	public class StatusBar : ControlBase, IFadeable
	{
		#region Static Interface

		static StatusBar()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusBar), new FrameworkPropertyMetadata(typeof(StatusBar)));
		}

		public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate),
			typeof(StatusBar), new PropertyMetadata(null, HandleDependencyPropertyChanged));

		public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector),
			typeof(StatusBar), new PropertyMetadata(null, HandleDependencyPropertyChanged));

		public static readonly DependencyProperty EntriesProperty = DependencyProperty.Register("Entries", typeof(IEnumerable),
			typeof(StatusBar), new PropertyMetadata(null, HandleDependencyPropertyChanged));

		public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string),
			typeof(StatusBar), new PropertyMetadata(string.Empty));

		public static readonly DependencyProperty StaysOpenProperty = DependencyProperty.Register("StaysOpen", typeof(bool),
			typeof(StatusBar), new PropertyMetadata(false));

		public static readonly DependencyProperty MaxPopupHeightProperty = DependencyProperty.Register("MaxPopupHeight", typeof(double),
			typeof(StatusBar), new PropertyMetadata(500.0));

		public static readonly DependencyProperty UnderlineBehaviorProperty = Link.UnderlineBehaviorProperty.AddOwner(typeof(StatusBar));

		public static readonly DependencyProperty UnderlineBrushProperty = Link.UnderlineBrushProperty.AddOwner(typeof(StatusBar));

		private static void HandleDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			StatusBar statusBar = (StatusBar)d;
			switch (e.Property.Name)
			{
				case "ItemTemplate":
				case "ItemTemplateSelector": statusBar.ApplyTemplating(); break;
			}
		}

		#endregion

		#region Dependency Properties

		public DataTemplate ItemTemplate
		{
			get => (DataTemplate)GetValue(ItemTemplateProperty);
			set => SetValue(ItemTemplateProperty, value);
		}

		public DataTemplateSelector ItemTemplateSelector
		{
			get => (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty);
			set => SetValue(ItemTemplateSelectorProperty, value);
		}

		public IEnumerable Entries
		{
			get => (IEnumerable)GetValue(EntriesProperty);
			set => SetValue(EntriesProperty, value);
		}

		public string DisplayMemberPath
		{
			get => (string)GetValue(DisplayMemberPathProperty);
			set => SetValue(DisplayMemberPathProperty, value);
		}

		public Link.LinkUnderlineBehavior UnderlineBehavior
		{
			get => (Link.LinkUnderlineBehavior)GetValue(UnderlineBehaviorProperty);
			set => SetValue(UnderlineBehaviorProperty, value);
		}

		public Brush UnderlineBrush
		{
			get => (Brush)GetValue(UnderlineBrushProperty);
			set => SetValue(UnderlineBrushProperty, value);
		}

		public bool StaysOpen
		{
			get => (bool)GetValue(StaysOpenProperty);
			set => SetValue(StaysOpenProperty, value);
		}

		public double MaxPopupHeight
		{
			get => (double)GetValue(MaxPopupHeightProperty);
			set => SetValue(MaxPopupHeightProperty, value);
		}

		#endregion

		private ContentControl _lastEntry = DefaultControls.ContentControl;
		private ItemsControl _pastEntries = DefaultControls.ItemsControl;
		private Link _link = Link.Default;

		protected override void OnDataContextChanged(object oldContext, object newContext)
		{
			base.OnDataContextChanged(oldContext, newContext);
			if (newContext is IStatusBarModel m) m.StatusBar = this;
		}

		protected override void UseTemplate()
		{
			_lastEntry = (ContentControl)GetTemplateChild("lastEntry");
			_pastEntries = (ItemsControl)GetTemplateChild("pastEntries");
			_link = (Link)GetTemplateChild("link");
			ApplyTemplating();
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			if (!StaysOpen && GetTemplateChild("popup") is Popup p && p.IsOpen) p.IsOpen = false;
		}

		private void ApplyTemplating()
		{
			if (!IsTemplateApplied) return;
			if (ItemTemplate != null)
			{
				if (ItemTemplateSelector != null) throw new InvalidOperationException($"Cannot set both {nameof(ItemTemplate)} and {nameof(ItemTemplateSelector)}.");
				_lastEntry.ContentTemplate = ItemTemplate;
				_pastEntries.ItemTemplate = ItemTemplate;
			}
			else
			if (ItemTemplateSelector != null)
			{
				if (ItemTemplate != null) throw new InvalidOperationException($"Cannot set both {nameof(ItemTemplate)} and {nameof(ItemTemplateSelector)}.");
				_lastEntry.ContentTemplateSelector = ItemTemplateSelector;
				_pastEntries.ItemTemplateSelector = ItemTemplateSelector;
			}
			else
			{
				_lastEntry.ContentTemplateSelector = _pastEntries.ItemTemplateSelector = null;
				_lastEntry.ContentTemplate = _pastEntries.ItemTemplate = null;
			}
		}

		internal void InitFade() => BeginFade?.Invoke(this, EventArgs.Empty);
		internal void CancelFade() => InterruptFade?.Invoke(this, EventArgs.Empty);
		void IFadeable.ResetFade()
		{

		}

		UIElement IFadeable.FadeTarget => _link;
		public event EventHandler? BeginFade;
		public event EventHandler? InterruptFade;
	}

	internal class StatusBarContentCoverter : IMultiValueConverter
	{
		public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values != null && values.Length == 2 && values[1] is string propName)
			{
				object? o = values[0];
				if (o is null) return string.Empty;
				PropertyInfo? pinfo = o.GetType().GetProperty(propName);
				if (pinfo == null) return string.Empty;
				object? o2 = pinfo.GetValue(o);
				return o2 == null ? string.Empty : o2.ToString();
			}
			return string.Empty;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
