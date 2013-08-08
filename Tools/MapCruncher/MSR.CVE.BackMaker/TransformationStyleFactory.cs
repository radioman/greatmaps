using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker
{
	public abstract class TransformationStyleFactory
	{
		private static List<TransformationStyle> transformationStyles = new List<TransformationStyle>();
		private static void init()
		{
			if (TransformationStyleFactory.transformationStyles.Count == 0)
			{
				TransformationStyleFactory.transformationStyles.Add(new AutomaticTransformationStyle());
				TransformationStyleFactory.transformationStyles.Add(new AffineTransformationStyle());
				TransformationStyleFactory.transformationStyles.Add(new HomographicTransformationStyle());
			}
		}
		public static TransformationStyle getTransformationStyle(int i)
		{
			TransformationStyleFactory.init();
			if (i < 0 || i >= TransformationStyleFactory.transformationStyles.Count)
			{
				i = 0;
			}
			return TransformationStyleFactory.transformationStyles[i];
		}
		public static TransformationStyle getDefaultTransformationStyle()
		{
			return TransformationStyleFactory.getTransformationStyle(0);
		}
		public static TransformationStyle ReadFromXMLAttribute(MashupParseContext context)
		{
			TransformationStyleFactory.init();
			string attribute = context.reader.GetAttribute(TransformationStyle.TransformationStyleNameAttr);
			if (attribute != null)
			{
				for (int i = 0; i < TransformationStyleFactory.transformationStyles.Count; i++)
				{
					if (TransformationStyleFactory.transformationStyles[i].getXmlName() == attribute)
					{
						return TransformationStyleFactory.transformationStyles[i];
					}
				}
				throw new InvalidMashupFile(context, string.Format("Invalid attribute value {1} for {0}", TransformationStyle.TransformationStyleNameAttr, attribute));
			}
			return TransformationStyleFactory.transformationStyles[0];
		}
	}
}
