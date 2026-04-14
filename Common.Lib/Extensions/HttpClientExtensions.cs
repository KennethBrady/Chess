namespace Common.Lib.Extensions
{
	public static class HttpClientExtensions
	{
		public static HttpClient Create()
		{
			HttpClientHandler ch = new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All };
			HttpClient r = new HttpClient(ch);
			r.MimicBrowserRequest();
			return r;
		}

		extension(HttpClient client)
		{
			public HttpClient MimicBrowserRequest()
			{
				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.106 Safari/537.36");
				client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
				client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
				client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

				;				return client;
			}
		}
	}
}
