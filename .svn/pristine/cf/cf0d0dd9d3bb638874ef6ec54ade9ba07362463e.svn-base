using System;
using System.Reflection;

namespace Sql.Lib.Services
{
	public class MissingAttributeException : Exception
	{
		internal MissingAttributeException(Type missing) : base($"Type {missing.Name} is missing the required {nameof(DBTableAttribute)}.")
		{
			AffectedType = missing;
		}

		public Type AffectedType { get; private init; }
	}

	public class UnmatchedFieldException : Exception
	{
		internal UnmatchedFieldException(PropertyInfo property) : base($"Unable to match property {property.Name} with a database field.")
		{
			Property = property;
		}

		public PropertyInfo Property { get; private init; }
	}

	public class WrongTypeException : Exception
	{
		internal WrongTypeException(Type expected, Type found): base($"Expected type {expected.Name}, but found {found.Name}.")
		{
			ExpectedType = expected;
			FoundType = found;
		}

		public Type ExpectedType
		{ get; private init; }
		public Type FoundType { get; private init; }
	}
}
