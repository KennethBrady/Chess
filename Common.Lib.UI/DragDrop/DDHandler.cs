using System.Windows;

namespace Common.Lib.UI.DragDrop
{
	/// <summary>
	/// Applies the strategy to a UIElement and handles DragEnter and Drop events
	/// </summary>
	public static class DDHandler
	{
		public static readonly DependencyProperty StrategyProperty = DependencyProperty.RegisterAttached("Strategy", typeof(IDDStrategy), 
			typeof(DDHandler), new PropertyMetadata(null, HandleStrategyChanged));

		private static void HandleStrategyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is UIElement uie)
			{
				if (e.NewValue is IDDStrategy strategy)
				{
					uie.AllowDrop = true;
					uie.SetValue(StrategyProperty, strategy);
					uie.DragEnter += Uie_DragEnter;
					uie.Drop += Uie_Drop;
				}
				else
				{
					uie.DragEnter -= Uie_DragEnter;
					uie.Drop -= Uie_Drop;
				}
			}
		}

		public static void SetStrategy(UIElement o, IDDStrategy? d) => o.SetValue(StrategyProperty, d);


		public static IDDStrategy? GetStrategy(UIElement o) => o.GetValue(StrategyProperty) as IDDStrategy;

		private static void Uie_DragEnter(object sender, DragEventArgs e)
		{
			IDDStrategy? s = GetStrategy((UIElement)sender);
			if (s != null && s.Accept(e.Data)) e.Effects = s.AllowedEffects;			
		}

		private static void Uie_Drop(object sender, DragEventArgs e)
		{
			IDDStrategy? d = GetStrategy((UIElement)sender);
			if (d == null) return;
			string? fmt = d.AcceptedFormats.FirstOrDefault(f => e.Data.GetDataPresent(f));
			if (fmt != null)
			{
				object o = e.Data.GetData(fmt);
				d.HandleDrop(o);
			}
		}
	}
}
