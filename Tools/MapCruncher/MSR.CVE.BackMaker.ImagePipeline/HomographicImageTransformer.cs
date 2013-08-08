using Jama;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Xml;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class HomographicImageTransformer : IImageTransformer
	{
		public HomographicImageTransformer(RegistrationDefinition registration, InterpolationMode interpolationMode) : base(registration, interpolationMode)
		{
			List<PositionAssociation> associationList = registration.GetAssociationList();
			TransformationStyle arg_15_0 = registration.warpStyle;
			int count = associationList.Count;
			JamaMatrix jamaMatrix = new JamaMatrix(count, 2);
			JamaMatrix jamaMatrix2 = new JamaMatrix(count, 2);
			for (int i = 0; i < count; i++)
			{
				LatLon latlon = associationList[i].sourcePosition.pinPosition.latlon;
				jamaMatrix.SetElement(i, 0, latlon.lon);
				jamaMatrix.SetElement(i, 1, latlon.lat);
				LatLon latLon = MercatorCoordinateSystem.LatLonToMercator(associationList[i].globalPosition.pinPosition.latlon);
				jamaMatrix2.SetElement(i, 0, latLon.lon);
				jamaMatrix2.SetElement(i, 1, latLon.lat);
			}
		}
		internal override void doTransformImage(GDIBigLockedImage sourceImage, MapRectangle sourceBounds, GDIBigLockedImage destImage, MapRectangle destBounds)
		{
		}
		internal override void writeToXml(XmlTextWriter writer)
		{
		}
	}
}
