using System;
namespace MSR.CVE.BackMaker
{
	public class SourceMapInfoAsCharDataSchema : MashupXMLSchemaVersion
	{
		public static SourceMapInfoAsCharDataSchema _schema;
		public static SourceMapInfoAsCharDataSchema schema
		{
			get
			{
				if (SourceMapInfoAsCharDataSchema._schema == null)
				{
					SourceMapInfoAsCharDataSchema._schema = new SourceMapInfoAsCharDataSchema();
				}
				return SourceMapInfoAsCharDataSchema._schema;
			}
		}
		private SourceMapInfoAsCharDataSchema() : base("1.3")
		{
		}
	}
}
