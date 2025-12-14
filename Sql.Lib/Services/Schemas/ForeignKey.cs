namespace Sql.Lib.Services.Schemas
{
	public class ForeignKey
	{
		internal ForeignKey(string constraintName, string refTableName, string refColName)
		{
			ConstraintName = constraintName;
			ReferencedTableName = refTableName;
			ReferencedColumnName = refColName;
		}

		public string ConstraintName { get; private set; }

		public string ReferencedTableName { get; private set; }

		public string ReferencedColumnName { get; private set; }

		public override string ToString()
		{
			return $"{ReferencedTableName}({ReferencedColumnName})";
		}
	}
}
