using System;
using System.Collections.Generic;

namespace Sql.Lib
{
	[Flags]
	public enum SqlOperation 
	{ 
		None		= 0x00, 
		Insert	= 0x01, 
		Update	= 0x02, 
		Delete	= 0x04,
		DeleteUpdate = Update | Delete,
		All			= Insert | Update | Delete
	};

	public interface ISqlResult
	{
		SqlOperation Operation { get; }
		object Input { get; }
		bool Succeeded { get; }
		string ExtraInfo { get; }
	}

	public interface ISqlResult<T> : ISqlResult where T : class
	{
		new T Input { get; }
	}

	#region Error / Update / Insert / Result interfaces

	public interface ISqlError : ISqlResult
	{
		string Error { get; }
	}

	public interface ISqlUpdateSuccess : ISqlResult
	{
		object Result { get; }
	}

	public interface ISqlInsertSuccess : ISqlResult
	{
		object Output { get; }
	}

	public interface ISqlError<T>: ISqlResult<T>, ISqlError where T : class;

	public interface ISqlErrorEx<T> : ISqlError<T> where T : class
	{
		public Exception Exception { get; }
	}

	public interface ISqlDeleteSuccess : ISqlResult;

	public interface ISqlDeleteSuccess<T> : ISqlDeleteSuccess, ISqlResult<T> where T : class { }

	public interface ISqlUpdateSuccess<T> : ISqlUpdateSuccess, ISqlResult<T> where T : class
	{
		new T Result { get; }
	}

	public interface ISqlInsertSuccess<T> : ISqlInsertSuccess, ISqlResult<T> where T : class
	{
		new T Output { get; }
	}

	public interface ISqlRowsChanged<T> where T : class
	{
		SqlOperation Operation => SqlOperation.DeleteUpdate;
		IEnumerable<T> Updates { get; }
		IEnumerable<T> Deletes { get; }
		int RowsUpdated { get; }
		int RowsDeleted { get; }
		bool Succeeded => true;
	}

	public interface ISqlRowsChangedError<T> : ISqlRowsChanged<T> where T : class
	{
		string Error { get; }
		new bool Succeeded => false;
	}

	#endregion

	public record struct SqlError<T>(SqlOperation Operation, T Input, string Error, string ExtraInfo = ""): ISqlError<T> where T : class
	{
		public bool Succeeded => false;

		object ISqlResult.Input => Input;
	}

	public record struct SqlResult(SqlOperation Operation, object Input, bool Succeeded, string ExtraInfo): ISqlResult;
	public record struct SqlResult<T>(SqlOperation Operation, T Input, bool Succeeded, string ExtraInfo): ISqlResult<T> where T : class
	{
		object ISqlResult.Input => Input;
	}

	public record struct SqlNoOp<T>(T Value, string ExtraInfo): ISqlResult<T> where T : class
	{
		public SqlOperation Operation => SqlOperation.None;
		public bool Succeeded => false;
		public object Input => Input;
		T ISqlResult<T>.Input => Value;
	}

	public record struct SqlErrorEx<T>(SqlOperation Operation, T Input, Exception Exception, string ExtraInfo = ""): ISqlErrorEx<T> where T : class
	{
		public string Error => Exception.Message;
		public bool Succeeded => false;

		object ISqlResult.Input => Input;
	}

	public record struct SqlDeleteSuccess<T>(T Input, string ExtraInfo = "") : ISqlDeleteSuccess<T> where T : class
	{
		public bool Succeeded => true;
		public SqlOperation Operation => SqlOperation.Delete;
		object ISqlResult.Input => Input;
	}

	public record struct SqlUpdateSuccess<T>(T Input, T Result, string ExtraInfo = ""): ISqlUpdateSuccess<T> where T : class
	{
		public SqlOperation Operation => SqlOperation.Update;
		public bool Succeeded => true;
		object ISqlResult.Input => Input;

		object ISqlUpdateSuccess.Result => Result;
	}

	public record struct SqlInsertSuccess<T>(T Input, T Output, string ExtraInfo = "") : ISqlInsertSuccess<T> where T : class
	{
		public bool Succeeded => true;
		public SqlOperation Operation => SqlOperation.Insert;
		object ISqlResult.Input => Input;

		object ISqlInsertSuccess.Output => Output;
	}

	public record struct SqlRowsChangedSuccess<T>(IEnumerable<T> Updates, IEnumerable<T> Deletes, int RowsUpdated, int RowsDeleted) : ISqlRowsChanged<T> where T : class { }

	public record struct SqlRowsChangedError<T>(IEnumerable<T> Updates, IEnumerable<T> Deletes, string Error) : ISqlRowsChangedError<T> where T : class
	{
		public bool Succeeded => false;
		public int RowsUpdated => 0;
		public int RowsDeleted => 0;
	}
}
