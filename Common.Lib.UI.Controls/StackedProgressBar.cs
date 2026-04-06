using Common.Lib.UI.Controls.Models;
using System.Windows;
using System.Windows.Controls;

namespace Common.Lib.UI.Controls
{
	public class StackedProgressBar : Control
	{
		static StackedProgressBar()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(StackedProgressBar), new FrameworkPropertyMetadata(typeof(StackedProgressBar)));
		}

		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(StackedValuesModel),
			typeof(StackedProgressBar), new PropertyMetadata(StackedValuesModel.Empty, null, CoerceModel));

		private static object CoerceModel(DependencyObject d, object value)
		{
			if (value == null) return StackedValuesModel.Empty;
			return value;
		}

		public StackedValuesModel Model
		{
			get => (StackedValuesModel)GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			switch (e.Property.Name)
			{
				case "Model":
					if (e.NewValue is StackedValuesModel svm)
					{
						svm.GetDimensions = GetDimensions;
						svm.Invoke = a => Dispatcher.Invoke(a);
					}
					break;
				case "DataContext": if (DataContext is StackedValuesModel m && !ReferenceEquals(m, Model)) Model = m; break;
			}
		}

		private (double Width, double Height) GetDimensions() => (ActualWidth, ActualHeight);

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			SizeChanged += StackedProgressBar_SizeChanged;
		}

		private void StackedProgressBar_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Model?.Resize();
		}
	}
}
