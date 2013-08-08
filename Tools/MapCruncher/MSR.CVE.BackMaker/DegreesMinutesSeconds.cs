using System;
using System.Text.RegularExpressions;
namespace MSR.CVE.BackMaker
{
	internal class DegreesMinutesSeconds
	{
		public enum OutputMode
		{
			DMS,
			DecimalDegrees
		}
		private Regex dmsRegex = new Regex("\r\n(?<Sign>-?)\r\n(?<Degrees>\\d+)\r\n\\s*\r\n([°dD])?\r\n\\s*\r\n\r\n(\r\n  (?<Minutes>\\d+)\r\n  \\s*\r\n  (['mM])?\r\n  \\s*\r\n\r\n  (\r\n    (?<Seconds>\\d+(\\.\\d+)?)\r\n    \\s*\r\n    ([\"sS])?\r\n  )?\r\n)?", RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
		public DegreesMinutesSeconds.OutputMode outputMode = DegreesMinutesSeconds.OutputMode.DecimalDegrees;
		public double ParseLatLon(string str)
		{
			try
			{
				double result = Convert.ToDouble(str);
				return result;
			}
			catch (FormatException)
			{
			}
			Match match = this.dmsRegex.Match(str);
			if (match.Success)
			{
				double num = (match.Groups["Sign"].Length > 0) ? -1.0 : 1.0;
				double num2 = Convert.ToDouble(match.Groups["Degrees"].Value);
				double num3 = 0.0;
				double num4 = 0.0;
				try
				{
					num3 = Convert.ToDouble(match.Groups["Minutes"].Value);
					try
					{
						num4 = Convert.ToDouble(match.Groups["Seconds"].Value);
					}
					catch
					{
					}
				}
				catch
				{
				}
				return num * (num2 + num3 * 0.016666666666666666 + num4 * 0.00027777777777777778);
			}
			throw new FormatException("Unable to parse lat/lon.");
		}
		public string FormatLatLon(double value)
		{
			string result;
			if (this.outputMode == DegreesMinutesSeconds.OutputMode.DMS)
			{
				int num = 1;
				double num2 = value;
				if (value < 0.0)
				{
					num = -1;
					num2 = -value;
				}
				int num3 = (int)num2;
				double num4 = num2 - (double)num3;
				int num5 = (int)(num4 * 60.0);
				double num6 = num4 * 60.0 - (double)num5;
				double num7 = num6 * 60.0;
				result = string.Format("{0:###}° {1}' {2:##.000}\"", num3 * num, num5, num7);
			}
			else
			{
				if (this.outputMode == DegreesMinutesSeconds.OutputMode.DecimalDegrees)
				{
					result = string.Format("{0:###.00000000}", value);
				}
				else
				{
					D.Assert(false, "Invalid enum value.");
					result = null;
				}
			}
			return result;
		}
	}
}
