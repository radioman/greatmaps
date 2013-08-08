using System;
namespace MSR.CVE.BackMaker
{
	public class SingleMaxZoomForEntireMashupSchema : MashupXMLSchemaVersion
	{
		public static SingleMaxZoomForEntireMashupSchema _schema;
		internal static string ZoomLevelsTag = "ZoomLevels";
		internal static string MinZoomTag = "MinZoom";
		internal static string MaxZoomTag = "MaxZoom";
		public static SingleMaxZoomForEntireMashupSchema schema
		{
			get
			{
				if (SingleMaxZoomForEntireMashupSchema._schema == null)
				{
					SingleMaxZoomForEntireMashupSchema._schema = new SingleMaxZoomForEntireMashupSchema();
				}
				return SingleMaxZoomForEntireMashupSchema._schema;
			}
		}
		private SingleMaxZoomForEntireMashupSchema() : base("1.4")
		{
		}
	}
}
