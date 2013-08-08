using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Xml;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public abstract class IImageTransformer
	{
		private class QualitySortPair : IComparable
		{
			public PositionAssociation assoc;
			public double fitQuality;
			public QualitySortPair(PositionAssociation assoc, double fitQuality)
			{
				this.assoc = assoc;
				this.fitQuality = fitQuality;
			}
			public int CompareTo(object obj)
			{
				if (!(obj is IImageTransformer.QualitySortPair))
				{
					return 1;
				}
				IImageTransformer.QualitySortPair qualitySortPair = (IImageTransformer.QualitySortPair)obj;
				int num = -this.fitQuality.CompareTo(qualitySortPair.fitQuality);
				if (num != 0)
				{
					return num;
				}
				return this.assoc.associationName.CompareTo(qualitySortPair.assoc.associationName);
			}
		}
		protected RegistrationDefinition registration;
		protected InterpolationMode interpolationMode;
		protected IPointTransformer destLatLonToSourceTransformer;
		protected IPointTransformer sourceToDestLatLonTransformer;
		public IImageTransformer(RegistrationDefinition registration, InterpolationMode interpolationMode)
		{
			this.registration = new RegistrationDefinition(registration, new DirtyEvent());
			this.interpolationMode = interpolationMode;
		}
		internal abstract void doTransformImage(GDIBigLockedImage sourceImage, MapRectangle sourceBounds, GDIBigLockedImage destImage, MapRectangle destBounds);
		internal IPointTransformer getDestLatLonToSourceTransformer()
		{
			return this.destLatLonToSourceTransformer;
		}
		internal IPointTransformer getSourceToDestLatLonTransformer()
		{
			return this.sourceToDestLatLonTransformer;
		}
		public RegistrationDefinition getWarpedRegistration()
		{
			IPointTransformer pointTransformer = this.getSourceToDestLatLonTransformer();
			List<PositionAssociation> associationList = this.registration.GetAssociationList();
			List<IImageTransformer.QualitySortPair> list = new List<IImageTransformer.QualitySortPair>();
			for (int i = 0; i < associationList.Count; i++)
			{
				PositionAssociation positionAssociation = associationList[i];
				bool invertError;
				LatLon p = pointTransformer.getTransformedPoint(positionAssociation.sourcePosition.pinPosition.latlon, out invertError);
				PositionAssociation positionAssociation2 = new PositionAssociation(positionAssociation.associationName, positionAssociation.imagePosition.pinPosition, new LatLonZoom(p.lat, p.lon, positionAssociation.sourcePosition.pinPosition.zoom), positionAssociation.globalPosition.pinPosition, new DirtyEvent());
				positionAssociation2.sourcePosition.invertError = invertError;
				positionAssociation2.sourcePosition.SetErrorPosition(DisplayablePosition.ErrorMarker.AsContributor, positionAssociation.globalPosition.pinPosition.latlon);
				positionAssociation2.pinId = positionAssociation.pinId;
				double num = LatLon.DistanceInMeters(p, positionAssociation.globalPosition.pinPosition.latlon);
				positionAssociation2.qualityMessage = LatLon.PrettyDistance(num);
				list.Add(new IImageTransformer.QualitySortPair(positionAssociation2, num));
			}
			list.Sort();
			RegistrationDefinition registrationDefinition = new RegistrationDefinition(new DirtyEvent());
			registrationDefinition.warpStyle = this.registration.warpStyle;
			foreach (IImageTransformer.QualitySortPair current in list)
			{
				registrationDefinition.AddAssociation(current.assoc);
			}
			registrationDefinition.isLocked = true;
			return registrationDefinition;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			this.registration.AccumulateRobustHash(hash);
			hash.Accumulate((int)this.interpolationMode);
		}
		internal abstract void writeToXml(XmlTextWriter writer);
	}
}
