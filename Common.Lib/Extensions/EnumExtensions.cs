using System.Text;

namespace Common.Lib.Extensions
{
	public static class EnumExtensions
	{
		extension(Enum e)
		{
			public string ToPascal()
			{
				StringBuilder s = new StringBuilder();
				foreach(char c in e.ToString())
				{
					if (s.Length == 0) s.Append(c); else
					{
						if (char.IsUpper(c)) s.Append(' ');
						s.Append(c);
					}
				}
				return s.ToString();
			}
		}
	}
}
