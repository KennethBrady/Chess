namespace Sql.Lib.Services
{
	internal interface ISqlStatement;

	internal record struct NoStatement : ISqlStatement
	{
		internal static readonly NoStatement Default = new NoStatement();
	}

	internal record struct SqlStatement(string Sql) : ISqlStatement;


}
