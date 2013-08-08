using System;
using System.Text;
namespace MSR.CVE.BackMaker
{
	public class StringUtils
	{
		public static string breakLines(string s)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 40;
			int num2 = 0;
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (c == '\r')
				{
					num2 = 0;
				}
				if (num2 > num)
				{
					stringBuilder.Append("\r\n_  ");
					num2 = 0;
				}
				stringBuilder.Append(c);
				num2++;
			}
			return stringBuilder.ToString();
		}
	}
}
