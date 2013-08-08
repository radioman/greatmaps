using System;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class CompositeBoundsVerb : Verb
	{
		public Present Evaluate(Present[] paramList)
		{
			MapRectangle mapRectangle = null;
			int i = 0;
			while (i < paramList.Length)
			{
				Present present = paramList[i];
				Present result;
				if (present is PresentFailureCode)
				{
					result = new PresentFailureCode((PresentFailureCode)present, "CompositeBoundsVerb");
				}
				else
				{
					if (present is BoundsPresent)
					{
						((BoundsPresent)present).GetRenderRegion().AccumulateBoundingBox(ref mapRectangle);
						i++;
						continue;
					}
					result = new PresentFailureCode(new Exception("Unexpected result of child computation in CompositeBoundsVerb"));
				}
				return result;
			}
			if (mapRectangle == null)
			{
				return new PresentFailureCode("No valid sourcemaps in input.");
			}
			RenderRegion renderRegion = new RenderRegion(mapRectangle, new DirtyEvent());
			return new BoundsPresent(renderRegion);
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("CompositeBoundsVerb");
		}
	}
}
