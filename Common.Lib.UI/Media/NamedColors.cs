using System.Reflection;
using System.Windows.Media;

namespace Common.Lib.UI.Media
{
	public static class NamedColors
	{
		private static readonly Dictionary<string, NamedColor> _namedColors = new Dictionary<string, NamedColor>();
		static NamedColors()
		{
			Type ct = typeof(Color);
			foreach (PropertyInfo pInfo in typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public))
			{
				if (pInfo.PropertyType == ct)
				{
					string name = pInfo.Name;
					if (pInfo.GetValue(null) is Color c)
					{
						_namedColors.Add(name, new NamedColor(name, c));
					}
				}
			}
		}

		public static IEnumerable<NamedColor> AllColors => _namedColors.Values;

		public static IEnumerable<Color> AllNamedColors => _namedColors.Values.Select(c => c.Color);

		public static NamedColor? NamedColorOf(Color c) => _namedColors.Values.FirstOrDefault(nc => nc.Color == c);

		public static IEnumerable<NamedColor> NamedColorsOf(IEnumerable<Color> colors)
		{
			foreach (Color c in colors)
			{
				NamedColor? nc = NamedColorOf(c);
				if (nc != null) yield return nc;
			}
		}

		public static NamedColor? ColorNamed(string name)
		{
			return _namedColors.ContainsKey(name) ? _namedColors[name] : null;
		}

		public static string ColorName(Color c)
		{
			foreach (var nvp in _namedColors)
			{
				if (nvp.Value.Color == c) return nvp.Key;
			}
			return string.Empty;
		}

		public class NamedColor : IComparable<NamedColor>
		{
			internal protected NamedColor(string name, Color color)
			{
				Name = name;
				Color = color;
			}

			public Color Color { get; init; }
			public string Name { get; init; }
			public Color Contrasting => ColorUtilities.Contrasting(Color);

			public override string ToString() => $"{Name} ({Color})";

			int IComparable<NamedColor>.CompareTo(NamedColor? other) => string.Compare(Name, other?.Name);
		}
	}
}
