using Common.Lib.Extensions;

namespace Common.Lib.UnitTests.Extensions
{
	[TestClass]
	public class StringExtensionsTest
	{
		[TestMethod]
		public void AreBalanced()
		{
			string s = "{";
			bool balanced = s.AreBalanced('{', '}', out int count);
			Assert.IsFalse(balanced);
			Assert.AreEqual(1, count);
			s = "{{";
			balanced = s.AreBalanced('{', '}', out count);
			Assert.IsFalse(balanced);
			Assert.AreEqual(2, count);
		}

		[TestMethod]
		public void SingleQuoteEscaped()
		{
			string s = "How's it goin'?";
			string sesc = s.SingleQuoteEscaped;
			Assert.AreEqual("How''s it goin''?", sesc);
			string unesc = sesc.SingleQuoteUnescaped;
			Assert.AreEqual(s, unesc);
		}

		[TestMethod]
		public void DoubleQuoted()
		{
			const string S = "Hello";
			Assert.AreEqual("\"Hello\"", S.DoubleQuoted);
		}

		[TestMethod]
		public void ConvertTo()
		{
			byte b = 5; sbyte sb = 5; short s = 5;
			ushort us = 5; DateTime d = DateTime.Now;
			object[] values = [5,5u,5L,b,sb,s,us,5f,5d,5m,true,d,"5",'5', new object()];
			foreach(var v in values)
			{
				string? sval = v.ToString();
				Assert.IsNotNull(sval);
				object? converted = sval.ConvertTo(v.GetType());
				Assert.IsNotNull(converted);
				switch(v)
				{
					case DateTime dt:
						{
							TimeSpan diff = dt - (DateTime)converted;
							int ms = Math.Abs(dt.Millisecond);
							Assert.IsLessThan(1000, ms);
						}
						break;
					case object o: Assert.IsInstanceOfType(converted, typeof(object)); break;
					default: Assert.AreEqual(v, converted, v.GetType().Name); break;
				}
			}
		}

	}
}
