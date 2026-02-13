using Common.Lib.UI.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Common.Lib.UI.Controls.Imaging
{
	public class ScaledImage : Image
	{
		private IInputElement? _inputParent;
		private IImageShifter? _shifter;

		public ScaledImage()
		{
			IsManipulationEnabled = true;
			ClipToBounds = true;
			SizeChanged += (o, e) =>
			{
				if (_shifter != null && e.NewSize.Width > 0 && e.NewSize.Height > 0)
				{
					_shifter.ImageSize = () => new Size(ActualWidth, ActualHeight);
				}
			};
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			ClipToBounds = true;
			ApplyTransformBindings();
		}

		private void ApplyTransformBindings()
		{
			TransformGroup tgrp = new TransformGroup();
			ScaleTransform stx = new ScaleTransform();
			Binding b = new Binding("Zoom");
			b.Mode = BindingMode.OneWay;
			BindingOperations.SetBinding(stx, ScaleTransform.ScaleXProperty, b);
			b = new Binding("Zoom");
			b.Mode = BindingMode.OneWay;
			BindingOperations.SetBinding(stx, ScaleTransform.ScaleYProperty, b);
			tgrp.Children.Add(stx);

			TranslateTransform tx = new TranslateTransform();
			b = new Binding("XShift") { Mode = BindingMode.OneWay };
			BindingOperations.SetBinding(tx, TranslateTransform.XProperty, b);
			b = new Binding("YShift") { Mode = BindingMode.OneWay };
			BindingOperations.SetBinding(tx, TranslateTransform.YProperty, b);
			tgrp.Children.Add(tx);

			RenderTransform = tgrp;
			_inputParent = ((Visual)this).FindParent((o) => o is IInputElement) as IInputElement;
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property.Name == "DataContext")
			{
				if(e.NewValue != null)
				{
					_shifter = e.NewValue as IImageShifter;
					if (_shifter == null) throw new ApplicationException($"{typeof(ScaledImage).Name} requires a DataContext implementing {typeof(IImageShifter).Name}.");
				}
			}
		}

		private IImageShifter? ImageShifter => _shifter;

		protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
		{
			base.OnManipulationStarting(e);
			var shifter = ImageShifter;
			List<(int id, Point p)> positions = e.Manipulators.Select(m => (m.Id, m.GetPosition(_inputParent))).ToList();
			if (positions.Count > 1) e.Mode = ManipulationModes.Scale;
			else
				e.Mode = ManipulationModes.Scale | ManipulationModes.Translate;
		}

		protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
		{
			base.OnManipulationStarted(e);
			var positions = e.Manipulators.Select(m => (m.Id, m.GetPosition(_inputParent))).ToList();
			//System.Diagnostics.Debug.WriteLine($"Start: {e.ManipulationOrigin.X:F1},{e.ManipulationOrigin.Y:F1}:  {PositionsOf(positions)}");
			ImageShifter?.StartShift(e.ManipulationOrigin, positions);
			e.Handled = true;
		}

		protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
		{
			base.OnManipulationDelta(e);
			var positions = e.Manipulators.Select(m => (m.Id, m.GetPosition(_inputParent))).ToList();
			//System.Diagnostics.Debug.WriteLine($"Delta: {e.ManipulationOrigin.X:F1},{e.ManipulationOrigin.Y:F1}:  {PositionsOf(positions)}");
			ImageShifter?.DeltaShift(positions);
			e.Handled = true;
		}

		protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
		{
			base.OnManipulationCompleted(e);
			ImageShifter?.EndShift();
			e.Handled = true;
		}
	}
}
