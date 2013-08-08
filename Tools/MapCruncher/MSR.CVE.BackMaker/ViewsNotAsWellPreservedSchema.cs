using System;
namespace MSR.CVE.BackMaker
{
	public class ViewsNotAsWellPreservedSchema : MashupXMLSchemaVersion
	{
		public static ViewsNotAsWellPreservedSchema _schema;
		public static ViewsNotAsWellPreservedSchema schema
		{
			get
			{
				if (ViewsNotAsWellPreservedSchema._schema == null)
				{
					ViewsNotAsWellPreservedSchema._schema = new ViewsNotAsWellPreservedSchema();
				}
				return ViewsNotAsWellPreservedSchema._schema;
			}
		}
		private ViewsNotAsWellPreservedSchema() : base("1.5")
		{
		}
	}
}
