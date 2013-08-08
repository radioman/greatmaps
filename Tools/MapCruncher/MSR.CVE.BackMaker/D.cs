using System;
namespace MSR.CVE.BackMaker
{
	public class D
	{
		private static int debug_level;
		private static bool assertionInProgress;
		public static void SetDebugLevel(int lvl)
		{
			D.debug_level = lvl;
		}
		public static void Say(int message_level, string s)
		{
			if (message_level <= D.debug_level)
			{
				Console.WriteLine(s);
			}
		}
		public static void Sayf(int message_level, string format_string, params object[] args)
		{
			if (message_level <= D.debug_level)
			{
				Console.WriteLine(string.Format(format_string, args));
			}
		}
		public static bool CustomPaintDisabled()
		{
			return D.assertionInProgress;
		}
		public static void Assert(bool condition, string message)
		{
			if (!condition)
			{
				D.assertionInProgress = true;
				D.Sayf(0, "Assertion failed: {0}", new object[]
				{
					message
				});
			}
			bool flag = false;
			if (flag)
			{
				D.assertionInProgress = false;
			}
		}
		public static void Assert(bool condition)
		{
			D.Assert(condition, "");
		}
	}
}
