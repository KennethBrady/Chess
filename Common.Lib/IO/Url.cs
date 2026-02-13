using System.Text;

namespace Common.Lib.IO
{


	public record struct Url(string Scheme, string Domain, string SubDomain, string QueryString)
	{
		public static readonly Url Empty = new Url(string.Empty, string.Empty, string.Empty, string.Empty);

		public static string DomainOf(string url) => Parse(url).Domain;

		public static Url Parse(string url)
		{
			if(string.IsNullOrEmpty(url)) return Empty;
			string scheme = string.Empty, domain = string.Empty, subdomain = string.Empty, qs = string.Empty;
			if (url.StartsWith("https://")) scheme = "https";
			else
				if (url.StartsWith("http://")) scheme = "http";
			if (!string.IsNullOrEmpty(scheme)) url = url.Substring(scheme.Length + 3);
			if (url.StartsWith("www.")) url = url.Substring(4);
			StringBuilder sb = new();
			int step = 0;
			foreach (char c in url)
			{
				switch (c)
				{
					case '/':
						switch (step)
						{
							case 0:
								domain = sb.ToString();
								sb.Clear();
								step = 1;
								continue;
							case 1:
							case 2: sb.Append(c); continue;								
						}
						break;
					case '?':
						switch (step)
						{
							case 0:
								domain = sb.ToString();
								sb.Clear();
								step = 2;
								continue;
							case 1:
								subdomain = sb.ToString();
								sb.Clear();
								step = 2;
								continue;
						}
						break;
					default: sb.Append(c); break;
				}
			}
			if (sb.Length > 0)
			{
				switch(step)
				{
					case 0: domain = sb.ToString(); break;
					case 1: subdomain = sb.ToString(); break;
					default: qs = sb.ToString(); break;
				}
			}
			return new Url(scheme, domain, subdomain, qs);
		}
	}
}
