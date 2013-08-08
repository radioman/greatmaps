using System;
namespace MSR.CVE.BackMaker
{
	public class CurrentSchema : MashupXMLSchemaVersion
	{
		public static CurrentSchema _schema;
		public static CurrentSchema schema
		{
			get
			{
				if (CurrentSchema._schema == null)
				{
					CurrentSchema._schema = new CurrentSchema();
				}
				return CurrentSchema._schema;
			}
		}
		private CurrentSchema() : base("1.7")
		{
		}
	}
}
