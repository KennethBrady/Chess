using System.Globalization;
using System.Windows.Media;

namespace Common.Lib.UI.Media
{
	public static class ColorUtilities
	{
		public static Color Interpolate(Color c1, Color c2, double distance, bool maximizeRange = false)
		{
			if (distance > 1.0) distance = 1;
			if (distance < 0.0) distance = 0;
			double dr = c2.R - c1.R, dg = c2.G - c1.G, db = c2.B - c1.B;
			double r = c1.R + dr * distance, g = c1.G + dg * distance, b = c1.B + db * distance;
			if (maximizeRange)
			{
				double min = Math.Min(r, Math.Min(g, b)), max = Math.Max(r, Math.Max(g, b));
				byte nrm(double v) => (byte)(255 * (v - min) / (max - min));
				r = nrm(r); g = nrm(g); b = nrm(b);
			}
			return Color.FromRgb((byte)r, (byte)g, (byte)b);
		}

		public static Color Contrasting(Color clr)
		{
			byte r = clr.R, g = clr.G, b = clr.B;
			r = FixC(r);
			g = FixC(g);
			b = FixC(b);
			return Color.FromRgb(r, g, b);
		}

		public static Color Parse(string sColor, Color? defaultColor = null)
		{
			if (String.IsNullOrEmpty(sColor))
			{
				if (defaultColor.HasValue) return defaultColor.Value;
				throw new ArgumentNullException(nameof(sColor));
			}
			if (sColor.StartsWith("#")) return ParseValue(sColor.Substring(1));
			if (sColor.StartsWith("0x")) return ParseValue(sColor.Substring(2));
			try
			{
				try { return ParseValue(sColor); }
				catch { }
				System.Drawing.Color c = System.Drawing.Color.FromName(sColor);
				if ((c.A == 0) && (sColor != "Transparent")) throw new ArgumentException(String.Format("'{0}' is not a known color.", sColor));
				return Color.FromArgb(c.A, c.R, c.G, c.B);
			}
			catch (ArgumentNullException) { throw; }
			catch (ArgumentException) { throw; }
			catch
			{
				throw new Exception("Unknown color: " + sColor);
			}
		}

		public static Color? ParseNull(string sColor)
		{
			if (string.IsNullOrEmpty(sColor)) return null;
			return TryParse(sColor, out Color c) ? c : null;
		}

		public static bool TryParse(string sColor, out Color c)
		{
			c = Colors.White;
			if (String.IsNullOrEmpty(sColor)) return false;
			try
			{
				c = Parse(sColor);
				return true;
			}
			catch
			{
				c = Colors.White;
				return false;
			}
		}

		public static string ToString(IEnumerable<Color> colors)
		{
			return (colors == null) ? string.Empty : string.Join(';', colors.Select(c => c.ToString()));
		}

		public static List<Color> ParseColors(string sColors)
		{
			List<Color> r = new List<Color>();
			if (sColors == null) return r;
			foreach (string sC in sColors.Split(';'))
				if (TryParse(sC, out Color c)) r.Add(c);
			return r;
		}

		public static List<Color> CreateGradient(Color c1, Color c2, int steps)
		{
			if (steps < 2) throw new ArgumentException("Must have at least two steps for a color gradient.");
			List<Color> list = new List<Color>();
			list.Add(c1);
			double dSteps = steps - 1;
			double r = c1.R, g = c1.G, b = c1.B, a = c1.A;
			double dr = (c2.R - c1.R) / dSteps, dg = (c2.G - c1.G) / dSteps, db = (c2.B - c1.B) / dSteps, da = (c2.A - c1.A) / dSteps;
			for (int i = 1; i < steps; ++i)
			{
				r += dr;
				g += dg;
				b += db;
				a += da;
				list.Add(Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b));
			}
			list.Add(c2);
			return list;
		}

		public static Color Attenuate(Color c, double attenuation)
		{
			if ((attenuation < 0) || (attenuation > 1)) throw new ArgumentException("Attenuation must be in the range [0, 1].", "attenuation");
			return Color.FromArgb((byte)(attenuation * c.A), c.R, c.G, c.B);
		}

		public static Color Lighten(this Color c, double amount = 0.1)
		{
			if (amount >= 1) return c;
			double amt = 1 + amount;
			byte ltn(byte b)
			{
				double r = b * amt;
				if (r > 255) r = 255;
				return (byte)r;
			}
			return Color.FromArgb(c.A, ltn(c.R), ltn(c.G), ltn(c.B));
		}

		public static Color Lighten(this Color c, double rAmt, double gAmt, double bAmt)
		{
			byte r = (byte)(Math.Min(255, rAmt * c.R)),
				g = (byte)(Math.Min(255, gAmt * c.G)),
				b = (byte)(Math.Min(255, bAmt * c.B));
			return Color.FromArgb(c.A, r, g, b);
		}

		public static Color Darken(this Color c, double amount) => c.Darken(amount, c.A);

		public static Color Darken(this Color c, double amount, byte a = 255)
		{
			if (amount >= 1.0) return Colors.Black;
			if (amount <= 0.0) return c;
			amount = 1 - amount;
			return Color.FromArgb(a, (byte)(amount * c.R), (byte)(amount * c.G), (byte)(amount * c.B));
		}

		private static Color ParseValue(string sColor)
		{
			long ic = long.Parse(sColor, NumberStyles.HexNumber);
			long a = (ic & 0xFF000000) >> 24, r = (ic & 0x00FF0000) >> 16, g = (ic & 0x0000FF00) >> 8, b = ic & 0x000000FF;
			if (sColor.Length == 6) a = 255;
			return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
		}

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

		private static readonly Color[] _rainbow = { Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Blue, Colors.Indigo, Colors.Violet };

		public static IEnumerable<Color> RainbowColors => _rainbow;

		/// <summary>
		/// Generate a proportional color in the rainbow.
		/// </summary>
		/// <param name="value">A value in the range [0..1]</param>
		/// <returns></returns>
		public static Color RainbowColorFromValue(double value)
		{
			if (value > 1.0) value = 1.0;
			if (value < 0) value = 0.0;
			double scaledValue = value * (_rainbow.Length - 1);
			int lower = (int)Math.Floor(scaledValue), upper = (int)Math.Ceiling(scaledValue);
			Color c1 = _rainbow[lower], c2 = _rainbow[upper];
			if (lower == upper) return c1;  // interpolation not necessary

			// r1, r2 are relative contributions of c1, c2 to the result:
			double r1 = upper - scaledValue, r2 = scaledValue - lower;
			byte toByte(byte b1, byte b2) => (byte)((r1 * b1) + (r2 * b2));
			byte r = toByte(c1.R, c2.R), g = toByte(c1.G, c2.G), b = toByte(c1.B, c2.B);
			return Color.FromRgb(r, g, b);
		}

		public static Color ColorFromValue(double value, params Color[] colors)
		{
			if (colors.Length == 0) return Colors.Transparent;
			if (colors.Length == 1) return colors[0];
			if (value > 1.0) value = 1.0;
			if (value < 0) value = 0.0;
			double scaledValue = value * (colors.Length - 1);
			int lower = (int)Math.Floor(scaledValue), upper = (int)Math.Ceiling(scaledValue);
			Color c1 = colors[lower], c2 = colors[upper];
			if (lower == upper) return c1;  // interpolation not necessary

			// r1, r2 are relative contributions of c1, c2 to the result:
			double r1 = upper - scaledValue, r2 = scaledValue - lower;
			byte toByte(byte b1, byte b2) => (byte)((r1 * b1) + (r2 * b2));
			byte r = toByte(c1.R, c2.R), g = toByte(c1.G, c2.G), b = toByte(c1.B, c2.B);
			return Color.FromRgb(r, g, b);
		}

		public static IEnumerable<Color> CreateGradient(int nSteps, params Color[] colors)
		{
			if (colors.Length == 0) throw new ArgumentException("At least 1 color must be provided.");
			if (nSteps < 2) throw new ArgumentException($"{nameof(nSteps)} must be > 1");
			if (colors.Length == 1)
			{
				for (int i = 0; i < nSteps; ++i) yield return colors[0];
			}
			for (int i = 0; i <= nSteps; ++i)
			{
				double value = (double)i / nSteps, scaledValue = value * (colors.Length - 1);
				int lower = (int)Math.Floor(scaledValue), upper = (int)Math.Ceiling(scaledValue);
				Color c1 = colors[lower], c2 = colors[upper];
				// r1, r2 are relative contributions of c1, c2 to the result:
				double r1 = upper - scaledValue, r2 = scaledValue - lower;
				byte toByte(byte b1, byte b2) => (byte)((r1 * b1) + (r2 * b2));
				byte r = toByte(c1.R, c2.R), g = toByte(c1.G, c2.G), b = toByte(c1.B, c2.B);
				yield return Color.FromRgb(r, g, b);
			}
		}

		public static IEnumerable<Color> RainbowColorSpectrum(int colorCount)
		{
			if (colorCount < 2) colorCount = 1;
			double delta = 1.0 / (colorCount - 1), v = 0;
			for (int i = 0; i < colorCount; ++i)
			{
				yield return RainbowColorFromValue(v);
				v += delta;
			}
		}
	}

	public static class ColorExtensions
	{
		extension(Color c)
		{
			public Color MakeTransparent(double amount)
			{
				if (amount > 1) amount = 1; else if (amount < 0) amount = 0;
				byte newAmount = (byte)(c.A * (1 - amount));
				return Color.FromArgb(newAmount, c.R, c.G, c.B);

			}
		}
	}
}
