using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
namespace MSR.CVE.BackMaker
{
	public class AffineTransformationStyle : TransformationStyle
	{
		public override IImageTransformer getImageTransformer(RegistrationDefinition registration, InterpolationMode interpolationMode)
		{
			return new PolynomialImageTransformer(registration, interpolationMode, 1);
		}
		public override int getCorrespondencesRequired()
		{
			return 2;
		}
		public override List<string> getDescriptionStrings(int numCorrespondences)
		{
			List<string> list = new List<string>();
			if (numCorrespondences >= 2)
			{
				list.Add("Ready to lock; add more points to increase accuracy.");
			}
			else
			{
				list.Add("Correspondences required:");
			}
			if (numCorrespondences < 2)
			{
				list.Add(string.Format("{0} more for rigid.", 2 - numCorrespondences));
			}
			if (numCorrespondences < 3)
			{
				list.Add(string.Format("{0} or more for affine.", 3 - numCorrespondences));
			}
			return list;
		}
		public override string getXmlName()
		{
			return "Affine";
		}
	}
}
