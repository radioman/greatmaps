using System;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class InvalidMashupFile : Exception
	{
		public InvalidMashupFile(MashupParseContext context, string msg) : this(context.reader, msg)
		{
		}
		public InvalidMashupFile(XmlTextReader reader, string msg) : base(string.Format("{0} at {1}", msg, MashupParseContext.FilePosition(reader)))
		{
		}
	}
}
