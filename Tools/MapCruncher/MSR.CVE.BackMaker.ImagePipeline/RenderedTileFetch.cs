using System;
using System.IO;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class RenderedTileFetch : Verb
	{
		private RenderedTileNamingScheme namingScheme;
		public RenderedTileFetch(RenderedTileNamingScheme namingScheme)
		{
			this.namingScheme = namingScheme;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("RenderedTileFetch(");
			this.namingScheme.AccumulateRobustHash(hash);
			hash.Accumulate(")");
		}
		public Present Evaluate(Present[] paramList)
		{
			Present result;
			try
			{
				TileAddress ta = (TileAddress)paramList[0];
				string renderPath = this.namingScheme.GetRenderPath(ta);
				if (File.Exists(renderPath))
				{
					GDIBigLockedImage gDIBigLockedImage = GDIBigLockedImage.FromFile(renderPath);
					gDIBigLockedImage.CopyPixels();
					result = new ImageRef(new ImageRefCounted(gDIBigLockedImage));
				}
				else
				{
					result = new BeyondImageBounds();
				}
			}
			catch (Exception ex)
			{
				result = new PresentFailureCode(ex);
			}
			return result;
		}
	}
}
