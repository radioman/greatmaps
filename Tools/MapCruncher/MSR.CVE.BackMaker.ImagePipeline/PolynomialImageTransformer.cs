using Jama;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Xml;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class PolynomialImageTransformer : IImageTransformer
	{
		private IPointTransformer destMercatorToSourceTransformer;
		private IPointTransformer sourceToDestMercatorTransformer_approximate;
		public PolynomialImageTransformer(RegistrationDefinition registration, InterpolationMode interpolationMode, int polynomialDegree) : base(registration, interpolationMode)
		{
			List<PositionAssociation> associationList = registration.GetAssociationList();
			TransformationStyle arg_15_0 = registration.warpStyle;
			int num = associationList.Count;
			if (num == 2)
			{
				num++;
			}
			JamaMatrix jamaMatrix = new JamaMatrix(num, 2);
			JamaMatrix jamaMatrix2 = new JamaMatrix(num, 2);
			for (int i = 0; i < num; i++)
			{
				LatLon latLon = (i == associationList.Count) ? PolynomialImageTransformer.getThirdPosition(associationList[0].sourcePosition.pinPosition.latlon, associationList[1].sourcePosition.pinPosition.latlon, true) : associationList[i].sourcePosition.pinPosition.latlon;
				jamaMatrix.SetElement(i, 0, latLon.lon);
				jamaMatrix.SetElement(i, 1, latLon.lat);
				LatLon latLon2 = (i == associationList.Count) ? PolynomialImageTransformer.getThirdPosition(MercatorCoordinateSystem.LatLonToMercator(associationList[0].globalPosition.pinPosition.latlon), MercatorCoordinateSystem.LatLonToMercator(associationList[1].globalPosition.pinPosition.latlon), false) : MercatorCoordinateSystem.LatLonToMercator(associationList[i].globalPosition.pinPosition.latlon);
				jamaMatrix2.SetElement(i, 0, latLon2.lon);
				jamaMatrix2.SetElement(i, 1, latLon2.lat);
			}
			this.destMercatorToSourceTransformer = PolynomialImageTransformer.getPolyPointTransformer(jamaMatrix, jamaMatrix2, polynomialDegree);
			this.sourceToDestMercatorTransformer_approximate = PolynomialImageTransformer.getApproximateInverterPolyPointTransformer(jamaMatrix, jamaMatrix2, polynomialDegree);
			DownhillInverterPointTransformer flakyPointTransformer = new DownhillInverterPointTransformer(this.destMercatorToSourceTransformer, this.sourceToDestMercatorTransformer_approximate);
			IPointTransformer sourceToMercator = new RobustPointTransformer(flakyPointTransformer, this.sourceToDestMercatorTransformer_approximate);
			this.destLatLonToSourceTransformer = new LatLonToSourceTransform(this.destMercatorToSourceTransformer);
			this.sourceToDestLatLonTransformer = new SourceToLatLonTransform(sourceToMercator);
		}
		internal override void doTransformImage(GDIBigLockedImage sourceImage, MapRectangle sourceBounds, GDIBigLockedImage destImage, MapRectangle destBounds)
		{
			MapRectangle inr = new MapRectangle(-0.5, -0.5, (double)destImage.Height - 0.5, (double)destImage.Width - 0.5);
			MapRectangle outr = MapRectangle.MapRectangleIgnoreOrder(MercatorCoordinateSystem.LatLonToMercator(destBounds.GetNW()), MercatorCoordinateSystem.LatLonToMercator(destBounds.GetSE()));
			JamaMatrix matrix = PolynomialImageTransformer.FindAffineMatrix(inr, outr);
			MapRectangle outr2 = new MapRectangle(-0.5, -0.5, (double)sourceImage.Height - 0.5, (double)sourceImage.Width - 0.5);
			JamaMatrix matrix2 = PolynomialImageTransformer.FindAffineMatrix(sourceBounds, outr2);
			FastImageWarper.doWarp(destImage, sourceImage, new IPointTransformer[]
			{
				new Affine2DPointTransformer(matrix),
				this.destMercatorToSourceTransformer,
				new Affine2DPointTransformer(matrix2)
			}, this.interpolationMode);
		}
		private static IPolyPointTransformer getPolyPointTransformer(JamaMatrix sourcePoints, JamaMatrix destPoints, int polynomialDegree)
		{
			JamaMatrix am = IPolyPointTransformer.Polynomialize(destPoints, polynomialDegree);
			JamaMatrix matrix = PolynomialImageTransformer.SVDSolveApply(am, PolynomialImageTransformer.PointUnroll(sourcePoints));
			switch (polynomialDegree)
			{
			case 1:
				return new FastPoly1PointTransformer(matrix);
			case 2:
				return new FastPoly2PointTransformer(matrix);
			default:
				return new SlowGeneralPolyPointTransformer(polynomialDegree, matrix);
			}
		}
		private static IPolyPointTransformer getApproximateInverterPolyPointTransformer(JamaMatrix sourcePoints, JamaMatrix destPoints, int polynomialDegree)
		{
			return PolynomialImageTransformer.getPolyPointTransformer(destPoints, sourcePoints, polynomialDegree);
		}
		private static LatLon getThirdPosition(LatLon p1, LatLon p2, bool senseNormal)
		{
			double num = p2.lon - p1.lon;
			double num2 = p2.lat - p1.lat;
			if (!senseNormal)
			{
				num *= -1.0;
				num2 *= -1.0;
			}
			return new LatLon(p1.lat + num, p1.lon - num2);
		}
		private static JamaMatrix CornersToVectorMatrix(MapRectangle rect)
		{
			JamaMatrix jamaMatrix = new JamaMatrix(3, 4);
			jamaMatrix.SetElement(0, 0, rect.lon0);
			jamaMatrix.SetElement(0, 1, rect.lon0);
			jamaMatrix.SetElement(0, 2, rect.lon1);
			jamaMatrix.SetElement(0, 3, rect.lon1);
			jamaMatrix.SetElement(1, 0, rect.lat0);
			jamaMatrix.SetElement(1, 1, rect.lat1);
			jamaMatrix.SetElement(1, 2, rect.lat0);
			jamaMatrix.SetElement(1, 3, rect.lat1);
			jamaMatrix.SetElement(2, 0, 1.0);
			jamaMatrix.SetElement(2, 1, 1.0);
			jamaMatrix.SetElement(2, 2, 1.0);
			jamaMatrix.SetElement(2, 3, 1.0);
			return jamaMatrix;
		}
		private static JamaMatrix FindAffineMatrix(MapRectangle inr, MapRectangle outr)
		{
			JamaMatrix gm = PolynomialImageTransformer.CornersToVectorMatrix(inr);
			return PolynomialImageTransformer.CornersToVectorMatrix(outr).times(PolynomialImageTransformer.PseudoInverseBySVD(gm));
		}
		private static JamaMatrix SVDSolveApply(JamaMatrix am, JamaMatrix b)
		{
			return PolynomialImageTransformer.PseudoInverseBySVD(am).times(b);
		}
		private static JamaMatrix PointUnroll(JamaMatrix pointVector)
		{
			JamaMatrix jamaMatrix = new JamaMatrix(pointVector.RowDimension * pointVector.ColumnDimension, 1);
			for (int i = 0; i < pointVector.ColumnDimension; i++)
			{
				jamaMatrix.setMatrix(pointVector.RowDimension * i, pointVector.RowDimension * (i + 1) - 1, 0, 0, pointVector.getMatrix(0, pointVector.RowDimension - 1, i, i));
			}
			return jamaMatrix;
		}
		private static JamaMatrix PointRoll(JamaMatrix unrolledVector, int numColumns)
		{
			int num = unrolledVector.RowDimension / numColumns;
			if (num * numColumns != unrolledVector.RowDimension)
			{
				throw new Exception("unrolledVector length not a multiple of numColumns");
			}
			if (unrolledVector.ColumnDimension != 1)
			{
				throw new Exception("unrolledVector not a column vector");
			}
			JamaMatrix jamaMatrix = new JamaMatrix(num, numColumns);
			for (int i = 0; i < numColumns; i++)
			{
				jamaMatrix.setMatrix(0, num - 1, i, i, unrolledVector.getMatrix(i * num, (i + 1) * num - 1, 0, 0));
			}
			return jamaMatrix;
		}
		private static JamaMatrix PseudoInverseBySVD(JamaMatrix gm)
		{
			if (gm.RowDimension < gm.ColumnDimension)
			{
				JamaMatrix gm2 = gm.transpose();
				JamaMatrix jamaMatrix = PolynomialImageTransformer.PseudoInverseBySVD(gm2);
				return jamaMatrix.transpose();
			}
			if (gm.RowDimension == gm.ColumnDimension)
			{
				return gm.inverse();
			}
			SingularValueDecomposition singularValueDecomposition = new SingularValueDecomposition(gm);
			JamaMatrix jamaMatrix2 = singularValueDecomposition.S.transpose();
			for (int i = 0; i < jamaMatrix2.RowDimension; i++)
			{
				double element = jamaMatrix2.GetElement(i, i);
				if (element != 0.0)
				{
					jamaMatrix2.SetElement(i, i, 1.0 / element);
				}
			}
			return singularValueDecomposition.getV().times(jamaMatrix2).times(singularValueDecomposition.getU().transpose());
		}
		private static JamaMatrix RegularUnitGrid(int size)
		{
			JamaMatrix jamaMatrix = new JamaMatrix(size * size, 2);
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					jamaMatrix.SetElement(i * size + j, 0, (double)j / (double)(size - 1));
					jamaMatrix.SetElement(i * size + j, 1, (double)i / (double)(size - 1));
				}
			}
			return jamaMatrix;
		}
		public static void TestFunc()
		{
			double[][] array = new double[4][];
			double[][] arg_15_0 = array;
			int arg_15_1 = 0;
			double[] array2 = new double[2];
			arg_15_0[arg_15_1] = array2;
			array[1] = new double[]
			{
				1.02,
				0.93
			};
			array[2] = new double[]
			{
				0.0,
				1.0
			};
			double[][] arg_73_0 = array;
			int arg_73_1 = 3;
			double[] array3 = new double[2];
			array3[0] = 1.0;
			arg_73_0[arg_73_1] = array3;
			double[][] a = array;
			JamaMatrix jamaMatrix = new JamaMatrix(a);
			PolynomialImageTransformer.RegularUnitGrid(4);
			JamaMatrix jamaMatrix2 = (JamaMatrix)jamaMatrix.Clone();
			jamaMatrix2.SetElement(1, 0, 1.0);
			jamaMatrix2.SetElement(1, 1, 1.0);
			JamaMatrix jamaMatrix3 = IPolyPointTransformer.Polynomialize(jamaMatrix, 2);
			JamaMatrix jamaMatrix4 = PolynomialImageTransformer.SVDSolveApply(jamaMatrix3, PolynomialImageTransformer.PointUnroll(jamaMatrix2));
			D.Say(0, "polyTransform:\n" + jamaMatrix4.ToString());
			JamaMatrix unrolledVector = jamaMatrix3.times(jamaMatrix4);
			D.Say(0, "testSolution:\n" + PolynomialImageTransformer.PointRoll(unrolledVector, 2).ToString());
			PolynomialImageTransformer.getPolyPointTransformer(jamaMatrix, jamaMatrix2, 2);
			LatLon p = new LatLon(0.93, 1.02);
			D.Say(0, "Invert test:\n" + PolynomialImageTransformer.getApproximateInverterPolyPointTransformer(jamaMatrix, jamaMatrix2, 2).getTransformedPoint(p).ToString());
		}
		public static void TestSVD()
		{
			int m = 8;
			int n = 5;
			JamaMatrix jamaMatrix = JamaMatrix.random(5, 3);
			JamaMatrix jamaMatrix2 = JamaMatrix.random(m, n).times(jamaMatrix).times(jamaMatrix.transpose());
			D.Say(0, "A = \n" + jamaMatrix2.ToString());
			SingularValueDecomposition singularValueDecomposition = new SingularValueDecomposition(jamaMatrix2);
			JamaMatrix u = singularValueDecomposition.getU();
			D.Say(0, "U = \n" + u.ToString());
			JamaMatrix s = singularValueDecomposition.S;
			D.Say(0, "S = \n" + s.ToString());
			JamaMatrix v = singularValueDecomposition.getV();
			D.Say(0, "V = \n" + v.ToString());
			D.Say(0, "rank = " + singularValueDecomposition.rank());
			D.Say(0, "cond = " + singularValueDecomposition.cond());
			D.Say(0, "norm2 = " + singularValueDecomposition.norm2());
			JamaMatrix jamaMatrix3 = new JamaMatrix(singularValueDecomposition.SingularValues, 1);
			D.Say(0, "singuler values = \n" + jamaMatrix3.ToString());
			JamaMatrix jamaMatrix4 = u.times(s).times(v.transpose());
			D.Say(0, "reconA =\n" + jamaMatrix4.ToString());
			JamaMatrix jamaMatrix5 = jamaMatrix4.minus(jamaMatrix2);
			D.Say(0, "diffA =\n" + jamaMatrix5.ToString());
		}
		internal override void writeToXml(XmlTextWriter writer)
		{
			writer.WriteStartElement("Transform");
			writer.WriteAttributeString("input", "SourceSpace");
			writer.WriteAttributeString("output", "Mercator");
			this.sourceToDestMercatorTransformer_approximate.writeToXml(writer);
			writer.WriteEndElement();
			writer.WriteStartElement("Transform");
			writer.WriteAttributeString("input", "Mercator");
			writer.WriteAttributeString("output", "SourceSpace");
			writer.WriteComment("This is the 'canonical' transform MapCruncher uses to warp its images. It numerically inverts this transform to compute its inverse. The SourceSpace->Mercator transform should be considered an approximation to the numerical inversion.");
			this.destMercatorToSourceTransformer.writeToXml(writer);
			writer.WriteEndElement();
		}
	}
}
