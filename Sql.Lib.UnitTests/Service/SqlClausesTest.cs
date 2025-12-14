using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sql.Lib.Services;

namespace Sql.Lib.UnitTests.Service
{
	[TestClass]
	public class SqlClausesTest
	{
		[TestMethod]
		public void WhereInList()
		{
			var inList = SqlClauses.WhereInList("id", 1, 2, 3, 4, 5);
			Assert.AreEqual("where id in (1,2,3,4,5)", inList.AsSql);
			Assert.AreEqual("where id not in (1,2,3,4,5)", inList.AsSqlNegated);
			Assert.AreEqual("id in (1,2,3,4,5)", inList.AsSqlWithoutWhere);
			Assert.AreEqual("id not in (1,2,3,4,5)", inList.AsSqlNegatedWithoutWhere);
		}

		[TestMethod]
		public void WhereInListWithQuotes()
		{
			var inList = SqlClauses.WhereInList("name", new string[] { "a", "b", "c" });
			Assert.AreEqual("where name in ('a','b','c')", inList.AsSql);
			Assert.AreEqual("where name not in ('a','b','c')", inList.AsSqlNegated);
			Assert.AreEqual("name in ('a','b','c')", inList.AsSqlWithoutWhere);
			Assert.AreEqual("name not in ('a','b','c')", inList.AsSqlNegatedWithoutWhere);
		}

		[TestMethod]
		public void Where()
		{
			var where = SqlClauses.Where("id>0");
			Assert.AreEqual("where id>0", where.AsSql);
			Assert.AreEqual("where not (id>0)", where.AsSqlNegated);

			where = SqlClauses.Where("id>0", "name is not null");
			Assert.AreEqual("where id>0 and name is not null", where.AsSql);

			where = SqlClauses.Where("id>0", "name is null", "age>21");
			Assert.AreEqual("where id>0 and name is null and age>21", where.AsSql);
		}
	}
}
