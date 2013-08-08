using System;
namespace MSR.CVE.BackMaker
{
	public class MC1NamingScheme : RenderedTileNamingScheme
	{
		public static string SchemeName = "MC1";
		public override string GetSchemeName()
		{
			return MC1NamingScheme.SchemeName;
		}
		public MC1NamingScheme(string filePrefix) : base(filePrefix, "png")
		{
		}
		public override string GetTileFilename(TileAddress ta)
		{
			return string.Format("z{0}\\y{1}\\x{2}{3}", new object[]
			{
				ta.ZoomLevel,
				ta.TileY,
				ta.TileX,
				this.fileSuffix
			});
		}
	}
}
