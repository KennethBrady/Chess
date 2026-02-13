namespace Common.Lib.Extensions
{
	/// <summary>
	/// These extensions help solve the various problems with floating-point comparisons.
	/// </summary>
	public static class DoubleExtensions
	{
		extension(double d)
		{
			public bool AlmostEquals(double dOther, double maxDifference = 1e-6) => Math.Abs(d - dOther) <= maxDifference;
		}

		extension(decimal d)
		{
			public bool AlmostEquals(decimal dOther, decimal maxDifference = 1e-6M) => Math.Abs(d - dOther) <= maxDifference;
		}
	}
}
