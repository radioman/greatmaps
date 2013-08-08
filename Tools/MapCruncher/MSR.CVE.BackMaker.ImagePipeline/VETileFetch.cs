using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class VETileFetch : WebTileFetch
	{
		private string mapStyle;
		public static string RoadStyle = "r";
		public static string AerialStyle = "a";
		public static string HybridStyle = "h";
		public VETileFetch(string mapStyle)
		{
			this.mapStyle = mapStyle;
		}
		public override void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("VETileFetch:");
			hash.Accumulate(this.mapStyle);
		}
		public static bool StyleIsValid(string s)
		{
			return s != null && (s == VirtualEarthWebDownloader.RoadStyle || s == VirtualEarthWebDownloader.AerialStyle || s == VirtualEarthWebDownloader.HybridStyle);
		}
		protected override string GetTileURL(TileAddress ta)
		{
			if (ta.ZoomLevel > 19)
			{
				return "ex: No VE Imagery at this Zoomlevel";
			}
			string text = "";
			int num = 0;
			for (int i = ta.ZoomLevel; i > 0; i--)
			{
				num = 0;
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
			string text2 = (this.mapStyle == "r") ? "png" : "jpeg";
			string formatString = VEUrlFormat.theFormat.GetFormatString();
			string text3 = string.Format(formatString, new object[]
			{
				this.mapStyle,
				num,
				text,
				text2,
				VEUrlFormat.theFormat.GetGenerationNumber()
			});
			D.Sayf(6, "getting VE url {0}", new object[]
			{
				text3
			});
			return text3;
		}
	}
}
