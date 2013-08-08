using System;
namespace MSR.CVE.BackMaker
{
	public class NoTagIdentities : MashupXMLSchemaVersion
	{
		public static NoTagIdentities _schema;
		public static NoTagIdentities schema
		{
			get
			{
				if (NoTagIdentities._schema == null)
				{
					NoTagIdentities._schema = new NoTagIdentities();
				}
				return NoTagIdentities._schema;
			}
		}
		private NoTagIdentities() : base("1.6")
		{
		}
	}
}
