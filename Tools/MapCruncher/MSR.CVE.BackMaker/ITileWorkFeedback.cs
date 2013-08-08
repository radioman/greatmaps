using MSR.CVE.BackMaker.ImagePipeline;
using System;
namespace MSR.CVE.BackMaker
{
	public interface ITileWorkFeedback
	{
		void PostMessage(string message);
		void PostComplaint(NonredundantRenderComplaint complaint);
		void PostImageResult(ImageRef image, Layer layer, string SourceMapName, TileAddress address);
	}
}
