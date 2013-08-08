using System;
namespace MSR.CVE.BackMaker
{
	public class InlineSourceMapInfoSchema : MashupXMLSchemaVersion
	{
		public static InlineSourceMapInfoSchema _schema;
		public static InlineSourceMapInfoSchema schema
		{
			get
			{
				if (InlineSourceMapInfoSchema._schema == null)
				{
					InlineSourceMapInfoSchema._schema = new InlineSourceMapInfoSchema();
				}
				return InlineSourceMapInfoSchema._schema;
			}
		}
		private InlineSourceMapInfoSchema() : base("1.2")
		{
		}
	}
}
