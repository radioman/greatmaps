using System;
namespace MSR.CVE.BackMaker
{
	public class MonolithicMapPositionsSchema : MashupXMLSchemaVersion
	{
		public const string monolithicMapPositionTag = "MapPosition";
		public const string monolithicLatAttr = "lat";
		public const string monolithicLonAttr = "lon";
		public const string monolithicZoomAttr = "zoom";
		public const string monolithicStyleAttr = "style";
		public static MonolithicMapPositionsSchema _schema;
		public static MonolithicMapPositionsSchema schema
		{
			get
			{
				if (MonolithicMapPositionsSchema._schema == null)
				{
					MonolithicMapPositionsSchema._schema = new MonolithicMapPositionsSchema();
				}
				return MonolithicMapPositionsSchema._schema;
			}
		}
		private MonolithicMapPositionsSchema() : base("1.0")
		{
		}
	}
}
