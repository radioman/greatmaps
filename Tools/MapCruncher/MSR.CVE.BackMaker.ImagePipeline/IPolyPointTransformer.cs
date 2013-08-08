using Jama;
using System;
using System.Globalization;
using System.Xml;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public abstract class IPolyPointTransformer : IPointTransformer
	{
		protected JamaMatrix transformCoefficients;
		protected int polynomialDegree;
		public IPolyPointTransformer(JamaMatrix transformCoefficients)
		{
			this.transformCoefficients = transformCoefficients;
		}
		public override void writeToXml(XmlTextWriter writer)
		{
			JamaMatrix jamaMatrix = IPolyPointTransformer.PolyExps(this.polynomialDegree);
			string[] array = new string[]
			{
				"x",
				"y"
			};
			for (int i = 0; i < 2; i++)
			{
				writer.WriteStartElement("Sum");
				writer.WriteAttributeString("Name", array[i]);
				for (int j = 0; j < jamaMatrix.RowDimension; j++)
				{
					writer.WriteStartElement("Term");
					writer.WriteAttributeString("Coefficient", this.transformCoefficients.GetElement(i * jamaMatrix.RowDimension + j, 0).ToString(CultureInfo.InvariantCulture));
					for (int k = 0; k < 2; k++)
					{
						writer.WriteAttributeString(array[k] + "_power", jamaMatrix.GetElement(j, k).ToString(CultureInfo.InvariantCulture));
					}
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
			}
		}
		public override IPointTransformer getInversePointTransfomer()
		{
			throw new NotImplementedException();
		}
		public static JamaMatrix Polynomialize(JamaMatrix values, int degree)
		{
			JamaMatrix jamaMatrix = IPolyPointTransformer.PolyExps(degree);
			JamaMatrix jamaMatrix2 = new JamaMatrix(values.RowDimension, jamaMatrix.RowDimension);
			for (int i = 0; i < jamaMatrix.RowDimension; i++)
			{
				for (int j = 0; j < values.RowDimension; j++)
				{
					jamaMatrix2.SetElement(j, i, Math.Pow(values.GetElement(j, 0), jamaMatrix.GetElement(i, 0)) * Math.Pow(values.GetElement(j, 1), jamaMatrix.GetElement(i, 1)));
				}
			}
			JamaMatrix jamaMatrix3 = new JamaMatrix(jamaMatrix2.RowDimension * 2, jamaMatrix2.ColumnDimension * 2);
			jamaMatrix3.setMatrix(0, jamaMatrix2.RowDimension - 1, 0, jamaMatrix2.ColumnDimension - 1, jamaMatrix2);
			jamaMatrix3.setMatrix(jamaMatrix2.RowDimension, 2 * jamaMatrix2.RowDimension - 1, jamaMatrix2.ColumnDimension, 2 * jamaMatrix2.ColumnDimension - 1, jamaMatrix2);
			return jamaMatrix3;
		}
		public static JamaMatrix PolyExps(int degree)
		{
			JamaMatrix jamaMatrix = new JamaMatrix((degree + 1) * (degree + 2) / 2, 2);
			int num = 0;
			for (int i = 0; i <= degree; i++)
			{
				for (int j = 0; j <= degree - i; j++)
				{
					jamaMatrix.SetElement(num, 0, (double)i);
					jamaMatrix.SetElement(num, 1, (double)j);
					num++;
				}
			}
			D.Assert(num == jamaMatrix.RowDimension);
			return jamaMatrix;
		}
	}
}
