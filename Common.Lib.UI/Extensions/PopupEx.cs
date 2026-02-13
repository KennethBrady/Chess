using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Common.Lib.UI.Extensions
{
	public class PopupEx
	{
		#region CloseOnEscape 
		public static DependencyProperty CloseOnEscapeProperty = DependencyProperty.RegisterAttached("CloseOnEscape",
			typeof(bool), typeof(PopupEx), new PropertyMetadata(false, HandleCloseOnEscapePropertyChanged));

		private static void HandleCloseOnEscapePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is Popup p)
			{
				if (GetCloseOnEscape(p)) p.Opened += Popup_Opened; else p.Opened -= Popup_Opened;
			}
		}

		private static void Popup_Opened(object? sender, EventArgs _)
		{
			Popup? p = sender as Popup;
			Window w = Window.GetWindow(p);
			void handleWindowKey(object sender, KeyEventArgs e)
			{
				if (e.Key == Key.Escape)
				{
					p?.IsOpen = false;
					w.KeyDown -= handleWindowKey;
				}
			}
			if (w != null && p != null) w.KeyDown += handleWindowKey;
		}

		public static bool GetCloseOnEscape(DependencyObject dobj)
		{
			object r = dobj.GetValue(CloseOnEscapeProperty);
			if (r is bool b) return b;
			return false;
		}

		public static void SetCloseOnEscape(DependencyObject dobj, bool closeOnEscape)
		{
			dobj.SetValue(CloseOnEscapeProperty, closeOnEscape);
		}

		#endregion

		#region Show / Hide popup when application activates / deactivates

		private class PopupMetadata
		{
			internal bool PopupWasOpen;
		}

		public static DependencyProperty HideOnAppDeactivateProperty = DependencyProperty.RegisterAttached("HideOnAppDeactivate",
			typeof(bool), typeof(PopupEx), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None,
				HandleHideOnDeactivatePropertyChanged));

		private static DependencyProperty PopupMetadataProperty = DependencyProperty.RegisterAttached("PopupMetadata", typeof(PopupMetadata),
			typeof(PopupEx), new FrameworkPropertyMetadata(null));

		private static void HandleHideOnDeactivatePropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
		{
			bool hoad = GetHideOnAppDeactivate(dobj);
			if (hoad && (dobj is Popup p))
			{
				p.Loaded += new RoutedEventHandler(HandlePopupLoaded);
			}
		}

		public static bool GetHideOnAppDeactivate(DependencyObject dobj)
		{
			object r = dobj.GetValue(HideOnAppDeactivateProperty);
			return (r == null) ? false : (bool)r;
		}

		public static void SetHideOnAppDeactivate(DependencyObject dobj, bool hoad)
		{
			dobj.SetValue(HideOnAppDeactivateProperty, hoad);
		}

		private static PopupMetadata? GetPopupAttached(DependencyObject dobj)
		{
			return dobj.GetValue(PopupMetadataProperty) as PopupMetadata;
		}

		private static void HandlePopupLoaded(object sender, RoutedEventArgs e)
		{
			Popup p = (Popup)sender;
			Window w = Window.GetWindow(p);
			if (w == null) return;
			PopupMetadata? pm = GetPopupAttached(p);
			if (pm != null) return;
			pm = new PopupMetadata();
			p.SetValue(PopupMetadataProperty, pm);
			w.Deactivated += new EventHandler(delegate (object? o, EventArgs ea)
			{
				pm.PopupWasOpen = p.IsOpen;
				if (pm.PopupWasOpen) p.IsOpen = false;
			});
			w.Activated += new EventHandler(delegate (object? o, EventArgs ea)
			{
				if (pm.PopupWasOpen) p.IsOpen = true;
				pm.PopupWasOpen = false;
			});
		}

		#endregion

	}
}
