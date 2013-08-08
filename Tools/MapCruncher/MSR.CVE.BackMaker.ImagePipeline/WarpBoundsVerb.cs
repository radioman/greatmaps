using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class WarpBoundsVerb : Verb
	{
		private IImageTransformer imageTransformer;
		public WarpBoundsVerb(IImageTransformer imageTransformer)
		{
			this.imageTransformer = imageTransformer;
		}
		public Present Evaluate(Present[] paramList)
		{
			D.Assert(paramList.Length == 1);
			IBoundsProvider boundsProvider = (IBoundsProvider)paramList[0];
			RenderRegion renderRegion = boundsProvider.GetRenderRegion();
			IPointTransformer robustPointTransform = this.imageTransformer.getSourceToDestLatLonTransformer();
			double num = 0.05;
			List<LatLon> asLatLonList = renderRegion.GetAsLatLonList();
			List<LatLon> list = new List<LatLon>();
			for (int i = 0; i < asLatLonList.Count; i++)
			{
				int index = (i + 1) % asLatLonList.Count;
				LatLon source = asLatLonList[i];
				LatLon dest = asLatLonList[index];
				ParametricLine parametricLine = new ParametricLine(source, dest);
				int numSteps = (int)Math.Max(1.0, parametricLine.Length() / num);
				List<LatLon> list2 = parametricLine.Interpolate(numSteps);
				list.AddRange(list2.ConvertAll<LatLon>((LatLon inp) => robustPointTransform.getTransformedPoint(inp)));
			}
			RenderRegion renderRegion2 = new RenderRegion(list, new DirtyEvent());
			return new BoundsPresent(renderRegion2);
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("WarpBoundsVerb(");
			this.imageTransformer.AccumulateRobustHash(hash);
			hash.Accumulate(")");
		}
	}
}
