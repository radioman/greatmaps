using System;
using System.Drawing;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class ImageRef : Present, IDisposable
	{
		private ImageRefCounted resource;
		private string refCredit;
		public GDIBigLockedImage image
		{
			get
			{
				return this.resource.image;
			}
		}
		public ImageRef(ImageRefCounted resource) : this(resource, "new")
		{
		}
		public ImageRef(ImageRefCounted resource, string refCredit)
		{
			resource.refCreditCounter++;
			this.refCredit = string.Format("{0}-{1}", refCredit, resource.refCreditCounter);
			resource.AddRef(this.refCredit);
			this.resource = resource;
		}
		public void Dispose()
		{
			this.resource.DropRef(this.refCredit);
		}
		public Present Duplicate(string refCredit)
		{
			return new ImageRef(this.resource, refCredit);
		}
		public ImageRef Copy()
		{
			GDIBigLockedImage image;
			Monitor.Enter(image = this.image);
			ImageRef result;
			try
			{
				Image original = this.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
				result = new ImageRef(new ImageRefCounted(new GDIBigLockedImage(new Bitmap(original))));
			}
			finally
			{
				Monitor.Exit(image);
			}
			return result;
		}
	}
}
