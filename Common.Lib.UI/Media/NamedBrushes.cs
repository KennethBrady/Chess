using System.Reflection;
using System.Windows.Media;

namespace Common.Lib.UI.Media
{
	public static class NamedBrushes
	{
		private static readonly Dictionary<string, NamedBrush> _namedBrushes;
		static NamedBrushes()
		{
			_namedBrushes = new Dictionary<string, NamedBrush>();
			Type bt = typeof(SolidColorBrush);
			foreach (PropertyInfo pi in typeof(Brushes).GetProperties(BindingFlags.Static | BindingFlags.Public))
			{
				if (pi.PropertyType == bt)
				{
					string name = pi.Name;
					if (pi.GetValue(null) is SolidColorBrush b) _namedBrushes.Add(name, new NamedBrush(name, b));
				}
			}
		}

		public static Brush? NamedColor(string name)
		{
			return _namedBrushes.ContainsKey(name) ? _namedBrushes[name].Brush : null;
		}

		public static string BrushName(Brush brush)
		{
			foreach (var nvp in _namedBrushes)
			{
				if (nvp.Value.Brush == brush) return nvp.Key;
			}
			return string.Empty;
		}

		public static string ColorName(Color color)
		{
			foreach (var nvp in _namedBrushes)
			{
				if (nvp.Value.Color == color) return nvp.Value.Name;
			}
			return string.Empty;
		}

		public static SolidColorBrush? BrushNamed(string name) => _namedBrushes.ContainsKey(name) ? _namedBrushes[name].Brush : null;

		public static IEnumerable<Color> NamedColors => _namedBrushes.Values.Select(b => b.Color);

		public static IEnumerable<NamedBrush> AllNamedBrushes => _namedBrushes.Values;

		private static byte FixC(byte b)
		{
			byte r = 0;
			if ((b >= 192) || (b <= 64)) r = (byte)(255 - b);
			else
				if (b < 64) r = (byte)(b + 128);
			else
				r = (byte)(b - 128);
			return r;
		}

		private static Color Contrasting(Color clr)
		{
			byte r = clr.R, g = clr.G, b = clr.B;
			r = FixC(r);
			g = FixC(g);
			b = FixC(b);
			return Color.FromRgb(r, g, b);
		}

		public class NamedBrush
		{
			internal NamedBrush(string name, SolidColorBrush brush)
			{
				Name = name;
				Brush = brush;
			}

			public string Name { get; private set; }
			public SolidColorBrush Brush { get; private set; }
			public Color Color => Brush.Color;
			public Brush ContrastingBrush => new SolidColorBrush(Contrasting(Brush.Color));
		}

		public class BrushServer
		{
			private SolidColorBrush[] _brushes;
			int _nBrush;
			public BrushServer(IEnumerable<SolidColorBrush> brushes) : this(brushes.ToArray()) { }

			public BrushServer(params SolidColorBrush[] brushes)
			{
				_brushes = brushes ?? throw new ArgumentNullException(nameof(brushes));
				if (_brushes.Length < 1) throw new ArgumentException("At least one brush must be provided.");
			}

			public BrushServer(IEnumerable<Color> colors) : this(colors.Select(c => new SolidColorBrush(c))) { }

			public IEnumerable<SolidColorBrush> AllBrushes => _brushes;

			public SolidColorBrush NextBrush()
			{
				if (_nBrush >= _brushes.Length) _nBrush = 0;
				return _brushes[_nBrush++];
			}

			public SolidColorBrush PeekBrush()
			{
				if (_nBrush >= _brushes.Length) _nBrush = 0;
				return _brushes[_nBrush];
			}

			public void Reset() => _nBrush = 0;
		}
	}
}
