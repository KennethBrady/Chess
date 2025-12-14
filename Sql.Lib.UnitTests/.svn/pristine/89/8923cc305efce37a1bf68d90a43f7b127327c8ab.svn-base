using Sql.Lib.Services;
using System;

namespace Sql.Lib.UnitTests.TestDB
{
	[DBTable("fileinfo", FilePathFields = "filepath")]
	public record class FInfo(int Id, string FilePath, DateTime FileDate, DateTime? RemoveDate, bool Maybe);
}
