using System;
namespace MSR.CVE.BackMaker
{
	public class VENamingScheme : RenderedTileNamingScheme
	{
		public static string SchemeName = "VE";
		public override string GetSchemeName()
		{
			return VENamingScheme.SchemeName;
		}
		public VENamingScheme(string filePrefix, string fileSuffix) : base(filePrefix, fileSuffix)
		{
		}
		public static string GetQuadKey(TileAddress ta)
		{
			string text = "";
			for (int i = ta.ZoomLevel; i > 0; i--)
			{
				int num = 0;
				int num2 = 1 << i - 1;
				if ((ta.TileX & num2) != 0)
				{
					num++;
				}
				if ((ta.TileY & num2) != 0)
				{
					num += 2;
				}
				text += num.ToString();
			}
			return text;
		}
		public override string GetTileFilename(TileAddress ta)
		{
			return string.Format("{0}{1}", VENamingScheme.GetQuadKey(ta), this.fileSuffix);
		}
	}
}
