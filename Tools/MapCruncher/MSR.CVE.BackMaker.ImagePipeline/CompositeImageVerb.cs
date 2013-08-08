using System;
using System.Drawing;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class CompositeImageVerb : Verb
	{
		public Present Evaluate(Present[] paramList)
		{
			Size value = ((SizeParameter)paramList[0]).value;
			GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage(value, "CompositeImageVerb");
			Present result;
			try
			{
				GDIBigLockedImage obj;
				Monitor.Enter(obj = gDIBigLockedImage);
				try
				{
					Graphics graphics = gDIBigLockedImage.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheGraphics();
					using (graphics)
					{
						for (int i = 1; i < paramList.Length; i++)
						{
							Present present = paramList[i];
							if (present is PresentFailureCode)
							{
								result = new PresentFailureCode((PresentFailureCode)present, "CompositeImageVerb");
								return result;
							}
							if (!(present is ImageRef))
							{
								result = new PresentFailureCode(new Exception("Unexpected result of child computation in CompositeImageVerb"));
								return result;
							}
							ImageRef imageRef = (ImageRef)present;
							GDIBigLockedImage image;
							Monitor.Enter(image = imageRef.image);
							try
							{
								graphics.DrawImage(imageRef.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage(), new Rectangle(0, 0, value.Width, value.Height));
							}
							finally
							{
								Monitor.Exit(image);
							}
						}
					}
				}
				finally
				{
					Monitor.Exit(obj);
				}
				ImageRef imageRef2 = new ImageRef(new ImageRefCounted(gDIBigLockedImage));
				gDIBigLockedImage = null;
				result = imageRef2;
			}
			finally
			{
				if (gDIBigLockedImage != null)
				{
					gDIBigLockedImage.Dispose();
				}
			}
			return result;
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("CompositeImageVerb");
		}
	}
}
