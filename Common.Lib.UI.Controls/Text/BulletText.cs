using System.Windows;
using System.Windows.Controls;

namespace Common.Lib.UI.Controls.Text
{
	public class BulletText : Control
	{
		static BulletText()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(BulletText), new FrameworkPropertyMetadata(typeof(BulletText)));
		}

		public static readonly DependencyProperty TextProperty = TextBlock.TextProperty.AddOwner(typeof(BulletText));
		public static readonly DependencyProperty TextWrappingProperty = TextBlock.TextWrappingProperty.AddOwner(typeof(BulletText));
		public static readonly DependencyProperty BulletProperty = DependencyProperty.Register("Bullet", typeof(string),
			typeof(BulletText), new PropertyMetadata(Unicode.Bullet));
		public static readonly DependencyProperty TextStyleProperty = DependencyProperty.Register("TextStyle", typeof(Style),
			typeof(BulletText), new PropertyMetadata(null, null, CoerceStyle));

		private static object? CoerceStyle(DependencyObject d, object value)
		{
			if (value is Style s)
			{
				if (s.TargetType == typeof(TextBlock)) return s;
			}
			return null;
		}

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public TextWrapping TextWrapping
		{
			get => (TextWrapping)GetValue(TextWrappingProperty);
			set => SetValue(TextWrappingProperty, value);
		}

		public string Bullet
		{
			get => (string)GetValue(BulletProperty);
			set => SetValue(BulletProperty, value);
		}

		public Style TextStyle
		{
			get => (Style)GetValue(TextStyleProperty);
			set => SetValue(TextStyleProperty, value);
		}
	}
}
