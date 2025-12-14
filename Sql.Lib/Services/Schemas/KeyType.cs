using System;

namespace Sql.Lib.Services.Schemas
{
	[Flags]
	public enum KeyType
	{
		None = 0,
		Primary = 0x01,
		Foreign = 0x02
	}
}
