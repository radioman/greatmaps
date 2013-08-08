using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public abstract class TransformationStyle : IRobustlyHashable
	{
		public static string TransformationStyleNameAttr = "WarpQuality";
		public abstract IImageTransformer getImageTransformer(RegistrationDefinition registration, InterpolationMode interpolationMode);
		public abstract int getCorrespondencesRequired();
		public abstract List<string> getDescriptionStrings(int numCorrespondences);
		public abstract string getXmlName();
		public void WriteXML(XmlTextWriter writer)
		{
			writer.WriteAttributeString(TransformationStyle.TransformationStyleNameAttr, this.getXmlName());
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(this.getXmlName());
		}
	}
}
