using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Common.Lib.UI.Controls.Imaging
{
	public class Dragimage : Control
	{
		#region Static Interface
		static Dragimage()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Dragimage), new FrameworkPropertyMetadata(typeof(Dragimage)));
		}

		public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double),
			typeof(Dragimage), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceZoom));

		public static readonly DependencyProperty AllowZoomProperty = DependencyProperty.Register("AllowZoom", typeof(bool),
			typeof(Dragimage), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty XShiftProperty = DependencyProperty.Register("XShift", typeof(double),
			typeof(Dragimage), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty AllowXShiftProperty = DependencyProperty.Register("AllowXShift", typeof(bool),
			typeof(Dragimage), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty YShiftProperty = DependencyProperty.Register("YShift", typeof(double),
			typeof(Dragimage), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty AllowYShiftProperty = DependencyProperty.Register("AllowYShift", typeof(bool),
			typeof(Dragimage), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty AllowRotationProperty = DependencyProperty.Register("AllowRotation", typeof(bool),
			typeof(Dragimage), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty RotationProperty = DependencyProperty.Register("Rotation", typeof(double),
			typeof(Dragimage), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceRotation));

		public static readonly DependencyProperty RotationConstraintProperty = DependencyProperty.Register("RotationConstraint", typeof(double),
			typeof(Dragimage), new PropertyMetadata(0.0, null, CoerceRotation));

		public static readonly DependencyProperty SourceProperty = Image.SourceProperty.AddOwner(typeof(Dragimage));

		public static readonly DependencyProperty StretchProperty = Image.StretchProperty.AddOwner(typeof(Dragimage));

		private static object CoerceZoom(DependencyObject d, object value)
		{
			if (value is double z && z > 0) return z;
			return 1.0;
		}

		private static object CoerceRotation(DependencyObject d, object value)
		{
			if (value is double v) return ConstrainRotation(v);
			return 0.0;
		}

		private static double ConstrainRotation(double rot)
		{
			rot %= 360.0;
			if (rot < 0) rot += 360.0;
			return rot;
		}

		#endregion

		#region Dependency Properties

		public bool AllowZoom
		{
			get => (bool)GetValue(AllowZoomProperty);
			set => SetValue(AllowZoomProperty, value);
		}

		public double Zoom
		{
			get => (double)GetValue(ZoomProperty);
			set => SetValue(ZoomProperty, value);
		}

		public bool AllowXShift
		{
			get => (bool)GetValue(AllowXShiftProperty);
			set => SetValue(AllowXShiftProperty, value);
		}

		public double XShift
		{
			get => (double)GetValue(XShiftProperty);
			set => SetValue(XShiftProperty, value);
		}

		public bool AllowYShift
		{
			get => (bool)GetValue(AllowYShiftProperty);
			set => SetValue(AllowYShiftProperty, value);
		}

		public double YShift
		{
			get => (double)GetValue(YShiftProperty);
			set => SetValue(YShiftProperty, value);
		}

		public bool AllowRotation
		{
			get => (bool)GetValue(AllowRotationProperty);
			set => SetValue(AllowRotationProperty, value);
		}

		public double Rotation
		{
			get => (double)GetValue(RotationProperty);
			set => SetValue(RotationProperty, value);
		}

		public double RotationConstraint
		{
			get => (double)GetValue(RotationConstraintProperty);
			set => SetValue(RotationConstraintProperty, value);
		}

		public Stretch Stretch
		{
			get => (Stretch)GetValue(StretchProperty);
			set => SetValue(StretchProperty, value);
		}

		public ImageSource Source
		{
			get => (ImageSource)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		#endregion

		private Image Image { get; set; } = DefaultControls.Image;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Image = (Image)GetTemplateChild("image");
			TransformGroup g = new TransformGroup();
			g.Children.Add(ScaleTransform);
			g.Children.Add(RotateTransform);
			g.Children.Add(TranslateTransform);
			Image.RenderTransform = g;
		}

		private ScaleTransform ScaleTransform { get; } = new ScaleTransform();
		private TranslateTransform TranslateTransform { get; } = new TranslateTransform();
		private RotateTransform RotateTransform { get; } = new RotateTransform { CenterX = 0.5, CenterY = 0.5 };

		private readonly record struct StartState(double XShift, double YShift, double Zoom, double Rotation);

		private bool IsManipulating { get; set; }
		protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
		{
			base.OnManipulationStarting(e);
			IsManipulating = true;
			ManipulationModes modes = ManipulationModes.None;
			if (AllowXShift) modes |= ManipulationModes.TranslateX;
			if (AllowYShift) modes |= ManipulationModes.TranslateY;
			if (AllowRotation) modes |= ManipulationModes.Rotate;
			if (AllowZoom) modes |= ManipulationModes.Scale;
			e.Mode = modes;
			e.IsSingleTouchEnabled = AllowXShift || AllowYShift;
			StartValues = new StartState(XShift, YShift, Zoom, Rotation);
		}

		private StartState StartValues { get; set; }

		protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
		{
			//System.Diagnostics.Debug.WriteLine($"Delta:\tTranslation:{e.CumulativeManipulation.Translation}\tScale:{e.CumulativeManipulation.Scale}\tRotation:{e.CumulativeManipulation.Rotation}");
			base.OnManipulationDelta(e);
			if (AllowXShift)
			{
				TranslateTransform.X = StartValues.XShift + e.CumulativeManipulation.Translation.X;
				SetCurrentValue(XShiftProperty, TranslateTransform.X);
			}
			if (AllowYShift)
			{
				TranslateTransform.Y = StartValues.YShift + e.CumulativeManipulation.Translation.Y;
				SetCurrentValue(YShiftProperty, TranslateTransform.Y);
			}
			if (AllowRotation) ApplyRotation(StartValues.Rotation + e.CumulativeManipulation.Rotation);
			if (AllowZoom)
			{
				double avgScale = (StartValues.Zoom * (e.CumulativeManipulation.Scale.X + e.CumulativeManipulation.Scale.Y) / 2.0) % 360.0;
				ScaleTransform.ScaleX = ScaleTransform.ScaleY = avgScale;
				SetCurrentValue(ZoomProperty, ScaleTransform.ScaleX);
			}
		}

		private void ApplyRotation(double rotation)
		{
			rotation = ConstrainRotation(rotation);
			double minRot = rotation;
			if (RotationConstraint > 0)
			{
				double minDiff = rotation;
				for (double a = 0.0; a <= 360.0; a += RotationConstraint)
				{
					double diff = Math.Abs(a - rotation);
					if (diff < minDiff)
					{
						minDiff = diff;
						minRot = a;
					}
				}
			}
			RotateTransform.Angle = minRot;
			SetCurrentValue(RotationProperty, RotateTransform.Angle);
		}

		protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
		{
			base.OnManipulationCompleted(e);
			IsManipulating = false;
		}

		// Apply external changes to transforms:
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (IsManipulating) return;
			switch (e.Property.Name)
			{
				case nameof(XShift): TranslateTransform.X = XShift; break;
				case nameof(YShift): TranslateTransform.Y = YShift; break;
				case nameof(Rotation): ApplyRotation(Rotation); break;
				case nameof(Zoom): ScaleTransform.ScaleX = ScaleTransform.ScaleY = Zoom; break;
			}
		}
	}
}
