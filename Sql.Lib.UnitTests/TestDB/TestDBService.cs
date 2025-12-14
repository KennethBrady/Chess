using Sql.Lib.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sql.Lib.UnitTests.TestDB
{
	internal static class TestDBService
	{
		public const string ConnectionString = "Server=localhost;Database=sqltests;Uid=tester;Pwd=jI7oXEN3;UseCompression=false;CharSet=utf8mb4";

		private static readonly MySqlService _service = new MySqlService(ConnectionString);

		internal static MySqlService Service => _service;

		internal static async Task<MySqlService> ReCreateAsync()
		{
			string fpath = Path.Combine(Environment.CurrentDirectory, "TestDb.sql");
			await _service.ExecuteScriptFileAsync(fpath, null);
			return new MySqlService(ConnectionString);
		}
	}
}
