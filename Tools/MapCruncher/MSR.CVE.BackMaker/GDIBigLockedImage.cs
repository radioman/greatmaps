using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
namespace MSR.CVE.BackMaker
{
	public class GDIBigLockedImage : IDisposable
	{
		public enum Transparentness
		{
			EntirelyTransparent,
			SomeOfEach,
			EntirelyOpaque
		}
		private Image gdiImage;
		private bool sizeKnown;
		private Size size;
		private Graphics gdiGraphics;
		private string sourceLabel;
		private static ResourceCounter allImageCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("GDIBigLockedImage", 10);
		private static Dictionary<string, ResourceCounter> fineGrainedImageCounter = new Dictionary<string, ResourceCounter>();
		public int Height
		{
			get
			{
				return this.Size.Height;
			}
		}
		public int Width
		{
			get
			{
				return this.Size.Width;
			}
		}
		public ImageFormat RawFormat
		{
			get
			{
				Monitor.Enter(this);
				ImageFormat rawFormat;
				try
				{
					rawFormat = this.gdiImage.RawFormat;
				}
				finally
				{
					Monitor.Exit(this);
				}
				return rawFormat;
			}
		}
		public Size Size
		{
			get
			{
				if (!this.sizeKnown)
				{
					Monitor.Enter(this);
					try
					{
						this.size = this.gdiImage.Size;
						this.sizeKnown = true;
					}
					finally
					{
						Monitor.Exit(this);
					}
				}
				return this.size;
			}
		}
		public GDIBigLockedImage(Size size, string sourceLabel)
		{
			this.gdiImage = new Bitmap(size.Width, size.Height);
			FIBR.Announce("GDIBigLockedImage.GDIBigLockedImage", new object[]
			{
				MakeObjectID.Maker.make(this),
				size
			});
			this.sourceLabel = sourceLabel;
			this.CrementCounter(1);
		}
		private void CrementCounter(int crement)
		{
			GDIBigLockedImage.allImageCounter.crement(crement);
			D.Assert(this.sourceLabel != null);
			if (!GDIBigLockedImage.fineGrainedImageCounter.ContainsKey(this.sourceLabel))
			{
				GDIBigLockedImage.fineGrainedImageCounter[this.sourceLabel] = DiagnosticUI.theDiagnostics.fetchResourceCounter("GDIBLI-" + this.sourceLabel, 10);
			}
			GDIBigLockedImage.fineGrainedImageCounter[this.sourceLabel].crement(crement);
		}
		public GDIBigLockedImage(Bitmap bitmap)
		{
			this.gdiImage = bitmap;
			this.sourceLabel = "bitmapCtor";
			this.CrementCounter(1);
		}
		private GDIBigLockedImage(string sourceLabel)
		{
			this.sourceLabel = sourceLabel;
			this.CrementCounter(1);
		}
		public static GDIBigLockedImage FromStream(Stream instream)
		{
			GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage("FromStream");
			gDIBigLockedImage.gdiImage = Image.FromStream(instream);
			FIBR.Announce("GDIBigLockedImage.FromStream", new object[]
			{
				MakeObjectID.Maker.make(gDIBigLockedImage)
			});
			return gDIBigLockedImage;
		}
		public static GDIBigLockedImage FromFile(string filename)
		{
			GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage("FromFile");
			gDIBigLockedImage.gdiImage = Image.FromFile(filename);
			FIBR.Announce("GDIBigLockedImage.FromFile", new object[]
			{
				MakeObjectID.Maker.make(gDIBigLockedImage),
				filename
			});
			return gDIBigLockedImage;
		}
		internal void CopyPixels()
		{
			Monitor.Enter(this);
			try
			{
				this.DisposeGraphics();
				Image image = new Bitmap(this.Size.Width, this.Size.Height);
				Graphics graphics = Graphics.FromImage(image);
				graphics.DrawImage(this.gdiImage, 0, 0, this.Size.Width, this.Size.Height);
				graphics.Dispose();
				this.gdiImage.Dispose();
				this.gdiImage = image;
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public void Dispose()
		{
			Monitor.Enter(this);
			try
			{
				if (this.gdiGraphics != null)
				{
					this.gdiGraphics.Dispose();
					FIBR.Announce("GDIBigLockedImage.Dispose(graphics)", new object[]
					{
						MakeObjectID.Maker.make(this)
					});
				}
				FIBR.Announce("GDIBigLockedImage.Dispose(image)", new object[]
				{
					MakeObjectID.Maker.make(this)
				});
				this.gdiImage.Dispose();
				this.CrementCounter(-1);
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public void DrawImageOntoThis(GDIBigLockedImage gDISImage, RectangleF destRect, RectangleF srcRect)
		{
			Monitor.Enter(this);
			try
			{
				Graphics gDIGraphics = this.GetGDIGraphics();
				Monitor.Enter(gDISImage);
				try
				{
					gDIGraphics.DrawImage(gDISImage.gdiImage, destRect, srcRect, GraphicsUnit.Pixel);
				}
				finally
				{
					Monitor.Exit(gDISImage);
				}
				this.gdiGraphics.Dispose();
				this.gdiGraphics = null;
				FIBR.Announce("GDIBigLockedImage.DrawImageOntoThis", new object[]
				{
					MakeObjectID.Maker.make(this),
					MakeObjectID.Maker.make(gDISImage),
					destRect,
					srcRect
				});
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		private Graphics GetGDIGraphics()
		{
			if (this.gdiGraphics == null)
			{
				Monitor.Enter(this);
				try
				{
					this.gdiGraphics = Graphics.FromImage(this.gdiImage);
					FIBR.Announce("GDIBigLockedImage.GetGDIGraphics", new object[]
					{
						MakeObjectID.Maker.make(this)
					});
				}
				finally
				{
					Monitor.Exit(this);
				}
			}
			return this.gdiGraphics;
		}
		public void Save(Stream outputStream, ImageFormat imageFormat)
		{
			Monitor.Enter(this);
			try
			{
				this.gdiImage.Save(outputStream, imageFormat);
				FIBR.Announce("GDIBigLockedImage.Save", new object[]
				{
					MakeObjectID.Maker.make(this),
					"stream"
				});
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public void Save(string outputFilename)
		{
			Monitor.Enter(this);
			try
			{
				this.gdiImage.Save(outputFilename);
				FIBR.Announce("GDIBigLockedImage.Save", new object[]
				{
					MakeObjectID.Maker.make(this),
					outputFilename
				});
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public Graphics IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheGraphics()
		{
			return this.GetGDIGraphics();
		}
		public Image IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage()
		{
			return this.gdiImage;
		}
		public void SetInterpolationMode(InterpolationMode interpolationMode)
		{
			Monitor.Enter(this);
			try
			{
				this.GetGDIGraphics().InterpolationMode = interpolationMode;
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public void SetClip(Region clipRegion)
		{
			Monitor.Enter(this);
			try
			{
				this.GetGDIGraphics().Clip = clipRegion;
				FIBR.Announce("GDIBigLockedImage.SetClip", new object[]
				{
					MakeObjectID.Maker.make(this)
				});
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		internal void DisposeGraphics()
		{
			Monitor.Enter(this);
			try
			{
				if (this.gdiGraphics != null)
				{
					this.gdiGraphics.Dispose();
					this.gdiGraphics = null;
					FIBR.Announce("GDIBigLockedImage.DisposeGraphics", new object[]
					{
						MakeObjectID.Maker.make(this)
					});
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public unsafe GDIBigLockedImage.Transparentness GetTransparentness()
		{
            Transparentness entirelyTransparent;
            lock (this)
            {
                Bitmap bitmap = (Bitmap)this.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                bool flag = false;
                bool condition = false;
                try
                {
                    PixelStruct* structPtr = (PixelStruct*)bitmapdata.Scan0;
                    for (int i = 0; i < bitmapdata.Height; i++)
                    {
                        for (int j = 0; j < bitmapdata.Width; j++)
                        {
                            PixelStruct* structPtr2 = (structPtr + ((i * bitmapdata.Stride) / sizeof(PixelStruct))) + j;
                            if (structPtr2->a != 0)
                            {
                                flag = true;
                            }
                            else
                            {
                                condition = true;
                            }
                            if (flag && condition)
                            {
                                return Transparentness.SomeOfEach;
                            }
                        }
                    }
                    if (flag)
                    {
                        D.Assert(!condition);
                        return Transparentness.EntirelyOpaque;
                    }
                    D.Assert(condition);
                    entirelyTransparent = Transparentness.EntirelyTransparent;
                }
                finally
                {
                    bitmap.UnlockBits(bitmapdata);
                }
            }
            return entirelyTransparent;
		}
	}
}
