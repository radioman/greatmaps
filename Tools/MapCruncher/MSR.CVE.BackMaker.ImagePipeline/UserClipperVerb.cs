using System;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	internal class UserClipperVerb : Verb
	{
		private CoordinateSystemIfc coordinateSystem = new MercatorCoordinateSystem();
		public Present Evaluate(Present[] paramList)
		{
			D.Assert(paramList.Length == 3);
			for (int i = 0; i < paramList.Length; i++)
			{
				Present present = paramList[i];
				if (present is PresentFailureCode)
				{
					return present;
				}
			}
			if (!(paramList[0] is ImageRef))
			{
				return paramList[0];
			}
			ImageRef imageRef = (ImageRef)paramList[0];
			TileAddress tileAddress = (TileAddress)paramList[1];
			BoundsPresent boundsPresent = (BoundsPresent)paramList[2];
			MapRectangle mapWindow = CoordinateSystemUtilities.TileAddressToMapRectangle(this.coordinateSystem, tileAddress);
			Region clipRegion = boundsPresent.GetRenderRegion().GetClipRegion(mapWindow, tileAddress.ZoomLevel, this.coordinateSystem);
			GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage(imageRef.image.Size, "UserClipperVerb");
			gDIBigLockedImage.SetClip(clipRegion);
			gDIBigLockedImage.DrawImageOntoThis(imageRef.image, new RectangleF(0f, 0f, (float)gDIBigLockedImage.Size.Width, (float)gDIBigLockedImage.Size.Height), new RectangleF(0f, 0f, (float)imageRef.image.Size.Width, (float)imageRef.image.Size.Height));
			ImageRef result = new ImageRef(new ImageRefCounted(gDIBigLockedImage));
			boundsPresent.Dispose();
			imageRef.Dispose();
			return result;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("UserClipperVerb()");
		}
	}
}
