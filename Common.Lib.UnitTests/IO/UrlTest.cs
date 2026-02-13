using Common.Lib.IO;

namespace Common.Lib.UnitTests.IO
{
	[TestClass]
	public class UrlTest
	{
		const string SURL = @"https://forecast.weather.gov/MapClick.php?lat=40.6265464&lon=-74.0915672";

		[TestMethod]
		public void UrlWorks()
		{
			Url url = Url.Parse(SURL);
			Assert.AreEqual("forecast.weather.gov", url.Domain);
			Assert.AreEqual("https", url.Scheme);
			Assert.AreEqual("MapClick.php", url.SubDomain);
			Assert.AreEqual("lat=40.6265464&lon=-74.0915672", url.QueryString);
		}

		[TestMethod]
		public void NoWWW()
		{
			const string surl = "www.kontiki.com";
			Url url = Url.Parse(surl);
			Assert.AreEqual("kontiki.com", url.Domain);
		}

		[TestMethod]
		public void DomainOf()
		{
			Assert.AreEqual("forecast.weather.gov", Url.DomainOf(SURL));
		}
	}
}
