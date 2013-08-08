using System;
namespace MSR.CVE.BackMaker
{
	public class InvalidLLZ : InvalidMashupFile
	{
		public InvalidLLZ(MashupParseContext context, string msg) : base(context, msg)
		{
		}
	}
}
