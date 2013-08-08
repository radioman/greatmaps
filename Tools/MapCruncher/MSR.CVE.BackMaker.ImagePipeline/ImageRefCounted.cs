using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Collections.Generic;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class ImageRefCounted
	{
		private GDIBigLockedImage _image;
		internal int refCreditCounter;
		private static ResourceCounter imageResourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("Image", 10);
		private int refs;
		private bool disposed;
		private List<string> _referenceHistory;
		public GDIBigLockedImage image
		{
			get
			{
				return this._image;
			}
		}
		private List<string> referenceHistory
		{
			get
			{
				if (this._referenceHistory == null)
				{
					this._referenceHistory = new List<string>();
				}
				return this._referenceHistory;
			}
		}
		public ImageRefCounted(GDIBigLockedImage image)
		{
			this._image = image;
			ImageRefCounted.imageResourceCounter.crement(1);
		}
		private void Dispose()
		{
			this._image.Dispose();
			this._image = null;
			ImageRefCounted.imageResourceCounter.crement(-1);
		}
		public void AddRef(string refCredit)
		{
			Monitor.Enter(this);
			try
			{
				D.Assert(!this.disposed);
				this.refs++;
				if (BuildConfig.theConfig.debugRefs)
				{
					this.referenceHistory.Add(string.Format("{0} Add  {1} refs={2}", MakeObjectID.Maker.make(this), refCredit, this.refs));
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public void DropRef(string refCredit)
		{
			Monitor.Enter(this);
			try
			{
				if (this.disposed)
				{
					if (BuildConfig.theConfig.debugRefs)
					{
						this.PrintReferenceHistory();
					}
					D.Assert(!this.disposed);
				}
				this.refs--;
				if (BuildConfig.theConfig.debugRefs)
				{
					this.referenceHistory.Add(string.Format("{0} Drop {1} refs={2}", MakeObjectID.Maker.make(this), refCredit, this.refs));
				}
				if (this.refs == 0)
				{
					this.Dispose();
					this.disposed = true;
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		private void PrintReferenceHistory()
		{
			foreach (string current in this.referenceHistory)
			{
				D.Sayf(0, "History: {0}", new object[]
				{
					current
				});
			}
		}
	}
}
