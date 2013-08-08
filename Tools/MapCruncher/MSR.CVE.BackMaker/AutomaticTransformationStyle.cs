using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
namespace MSR.CVE.BackMaker
{
	public class AutomaticTransformationStyle : TransformationStyle
	{
		private const int correspondencesRequiredForQuadratic = 7;
		public override IImageTransformer getImageTransformer(RegistrationDefinition registration, InterpolationMode interpolationMode)
		{
			return new PolynomialImageTransformer(registration, interpolationMode, this.getPolynomialDegree(registration.GetAssociationList().Count));
		}
		public override int getCorrespondencesRequired()
		{
			return 2;
		}
		protected int getPolynomialDegree(int numCorrespondences)
		{
			if (numCorrespondences < 7)
			{
				return 1;
			}
			return 2;
		}
		public override List<string> getDescriptionStrings(int numCorrespondences)
		{
			List<string> list = new List<string>();
			if (numCorrespondences >= this.getCorrespondencesRequired())
			{
				list.Add("Ready to lock; add more points to increase accuracy.");
			}
			else
			{
				list.Add("Correspondences required:");
			}
			if (numCorrespondences < 2)
			{
				list.Add(string.Format("At least {0} more for rigid.", 2 - numCorrespondences));
			}
			if (numCorrespondences < 3)
			{
				list.Add(string.Format("At least {0} more for affine.", 3 - numCorrespondences));
			}
			if (numCorrespondences < 7)
			{
				list.Add(string.Format("At least {0} more for quadratic.", 7 - numCorrespondences));
			}
			return list;
		}
		public override string getXmlName()
		{
			return "Automatic";
		}
	}
}
