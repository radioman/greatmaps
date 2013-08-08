using System;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class SourceMapRendererTools
	{
		public static RectangleF ToSquare(RectangleF box)
		{
			RectangleF result = new RectangleF(box.Location, box.Size);
			if (box.Height < box.Width)
			{
				result.Height = box.Width;
			}
			else
			{
				if (box.Width < box.Height)
				{
					result.Width = box.Height;
				}
			}
			return result;
		}
	}
}
