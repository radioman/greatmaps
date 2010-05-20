
namespace GMap.NET.WindowsForms.Markers
{
   using System;
   using System.Drawing;
   using System.Drawing.Imaging;
   using System.IO;
   using System.Runtime.InteropServices;

   public abstract class GMapMarkerTransparent : GMapMarker
   {
      protected GMapMarkerTransparent(PointLatLng pos)
         : base(pos)
      {
      }

      public override void OnRender(Graphics g)
      {
         for(Int32 i = 0; i < this.BitmapCount; i++)
         {
            IGMapTransparentBitmap bitmap = GetBitmap(i);
            if(bitmap == null)
            {
               continue;
            }

            System.Drawing.Rectangle src = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Rectangle dst = new System.Drawing.Rectangle(LocalPosition.X + bitmap.DrawOffset.X, LocalPosition.Y + bitmap.DrawOffset.Y, bitmap.Width, bitmap.Height);
            bitmap.Draw(g, dst, src);
         }
      }

      #region Abstract Members

      protected abstract Int32 BitmapCount
      {
         get;
      }
      protected abstract IGMapTransparentBitmap GetBitmap(Int32 index);

      #endregion

      #region Load Bitmap

      public static IGMapTransparentBitmap LoadTransparentBitmap(Stream stream)
      {
         return LoadTransparentBitmap(stream, true);
      }

      public static IGMapTransparentBitmap LoadTransparentBitmap(Stream stream, Boolean hasAlpha)
      {
         try
         {
            if(hasAlpha && Environment.OSVersion.Platform == PlatformID.WinCE)
               return new WinCEImagingBitmap(stream);
            return new StandardBitmap(stream);
         }
         catch(Exception)
         {
            return null;
         }
      }

      #endregion
   }

   #region Transparent Bitmap Wrappers

   public interface IGMapTransparentBitmap : IDisposable
   {
      Int32 Width
      {
         get;
      }
      Int32 Height
      {
         get;
      }
      Point DrawOffset
      {
         get;
         set;
      }
      void Draw(Graphics graphics, System.Drawing.Rectangle destRect, System.Drawing.Rectangle sourceRect);
   }

   public class WinCEImagingBitmap : IGMapTransparentBitmap
   {
      IImage myImage;
      ImageInfo myInfo;
      double myScaleFactorX = 0;
      double myScaleFactorY = 0;
      IntPtr myBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RECT)));
      static IImagingFactory myImagingFactory;

      public WinCEImagingBitmap(Stream stream)
      {
         // this class should only be used in WinCE
         System.Diagnostics.Debug.Assert(Environment.OSVersion.Platform == PlatformID.WinCE);

         if(myImagingFactory == null)
            myImagingFactory = (IImagingFactory) Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("327ABDA8-072B-11D3-9D7B-0000F81EF32E")));

         int bytesLength;
         byte[] bytes;
         MemoryStream memStream = stream as MemoryStream;
         if(memStream != null)
         {
            bytesLength = (int) memStream.Length;
            bytes = memStream.GetBuffer();
         }
         else
         {
            bytesLength = (int) stream.Length;
            bytes = new byte[bytesLength];
            stream.Read(bytes, 0, bytesLength);
         }

         uint hresult = myImagingFactory.CreateImageFromBuffer(bytes, (uint) bytesLength, BufferDisposalFlag.BufferDisposalFlagNone, out myImage);
         myImage.GetImageInfo(out myInfo);
         myScaleFactorX = 1 / myInfo.Xdpi * 2540;
         myScaleFactorY = 1 / myInfo.Ydpi * 2540;

         IBitmapImage bitmap;
         myImagingFactory.CreateBitmapFromImage(myImage, 0, 0, PixelFormatID.PixelFormat32bppARGB, InterpolationHint.InterpolationHintDefault, out bitmap);
         Marshal.FinalReleaseComObject(myImage);
         myImage = bitmap as IImage;
      }

      #region IGMapTransparentBitmap Members

      public Int32 Width
      {
         get
         {
            return myInfo.Width;
         }
      }

      public Int32 Height
      {
         get
         {
            return myInfo.Height;
         }
      }

      Point drawOffset = new Point(0, 0);

      public Point DrawOffset
      {
         get
         {
            return drawOffset;
         }
         set
         {
            drawOffset = value;
         }
      }

      public void Draw(Graphics graphics, System.Drawing.Rectangle destRect, System.Drawing.Rectangle sourceRect)
      {
         RECT dst = new RECT(destRect);

         if(destRect == sourceRect)
         {
            IntPtr hdc = graphics.GetHdc();
            myImage.Draw(hdc, ref dst, IntPtr.Zero);
            graphics.ReleaseHdc(hdc);
         }
         else
         {
            RECT src = new RECT(sourceRect);
            src.Left = (int) (src.Left * myScaleFactorX);
            src.Top = (int) (src.Top * myScaleFactorY);
            src.Right = (int) (src.Right * myScaleFactorX);
            src.Bottom = (int) (src.Bottom * myScaleFactorY);
            Marshal.StructureToPtr(src, myBuffer, false);
            IntPtr hdc = graphics.GetHdc();
            myImage.Draw(hdc, ref dst, myBuffer);
            graphics.ReleaseHdc(hdc);
         }
      }

      #endregion

      #region IDisposable Members

      public void Dispose()
      {
         if(myImage != null)
         {
            Marshal.FinalReleaseComObject(myImage);
            myImage = null;
         }
         if(myBuffer != IntPtr.Zero)
         {
            Marshal.FreeHGlobal(myBuffer);
         }
      }

      #endregion
   }

   public class StandardBitmap : IGMapTransparentBitmap
   {
      Bitmap myBitmap;

      public Bitmap Bitmap
      {
         get
         {
            return myBitmap;
         }
         set
         {
            myBitmap = value;
         }
      }

      ImageAttributes myImageAttributes;
      public StandardBitmap(Bitmap bitmap, ImageAttributes imageAttributes)
      {
         if(Environment.OSVersion.Platform == PlatformID.WinCE)
         {
            // reduce the bpp to native WinCE resolution for performance
            myBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format16bppRgb565);
            using(Graphics g = Graphics.FromImage(myBitmap))
            {
               g.DrawImage(bitmap, 0, 0);
            }
         }
         else
         {
            myBitmap = bitmap;
         }
         myImageAttributes = imageAttributes;
      }

      public StandardBitmap(Stream bitmapStream, ImageAttributes imageAttributes)
         : this(new Bitmap(bitmapStream), imageAttributes)
      {
      }

      public StandardBitmap(Stream bitmapStream)
         : this(bitmapStream, null)
      {
      }

      public StandardBitmap(Bitmap bitmap)
         : this(bitmap, null)
      {
      }

      #region IGMapTransparentBitmap Members

      public Int32 Width
      {
         get
         {
            return myBitmap.Width;
         }
      }

      public Int32 Height
      {
         get
         {
            return myBitmap.Height;
         }
      }

      Point drawOffset = new Point(0, 0);

      public Point DrawOffset
      {
         get
         {
            return drawOffset;
         }
         set
         {
            drawOffset = value;
         }
      }

      public void Draw(Graphics graphics, System.Drawing.Rectangle destRect, System.Drawing.Rectangle sourceRect)
      {
         if(myImageAttributes == null)
            graphics.DrawImage(myBitmap, destRect, sourceRect, GraphicsUnit.Pixel);
         else
            graphics.DrawImage(myBitmap, destRect, sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height, GraphicsUnit.Pixel, myImageAttributes);
      }

      #endregion

      #region IDisposable Members

      public void Dispose()
      {
         if(myBitmap != null)
         {
            myBitmap.Dispose();
            myBitmap = null;
         }
      }

      #endregion
   }

   #endregion

   #region IImage

   // Pulled from gdipluspixelformats.h in the Windows Mobile 5.0 Pocket PC SDK
   enum PixelFormatID : int
   {
      PixelFormatIndexed=0x00010000, // Indexes into a palette
      PixelFormatGDI=0x00020000, // Is a GDI-supported format
      PixelFormatAlpha=0x00040000, // Has an alpha component
      PixelFormatPAlpha=0x00080000, // Pre-multiplied alpha
      PixelFormatExtended=0x00100000, // Extended color 16 bits/channel
      PixelFormatCanonical=0x00200000,

      PixelFormatUndefined=0,
      PixelFormatDontCare=0,

      PixelFormat1bppIndexed=(1 | (1 << 8) | PixelFormatIndexed | PixelFormatGDI),
      PixelFormat4bppIndexed=(2 | (4 << 8) | PixelFormatIndexed | PixelFormatGDI),
      PixelFormat8bppIndexed=(3 | (8 << 8) | PixelFormatIndexed | PixelFormatGDI),
      PixelFormat16bppRGB555=(5 | (16 << 8) | PixelFormatGDI),
      PixelFormat16bppRGB565=(6 | (16 << 8) | PixelFormatGDI),
      PixelFormat16bppARGB1555=(7 | (16 << 8) | PixelFormatAlpha | PixelFormatGDI),
      PixelFormat24bppRGB=(8 | (24 << 8) | PixelFormatGDI),
      PixelFormat32bppRGB=(9 | (32 << 8) | PixelFormatGDI),
      PixelFormat32bppARGB=(10 | (32 << 8) | PixelFormatAlpha | PixelFormatGDI | PixelFormatCanonical),
      PixelFormat32bppPARGB=(11 | (32 << 8) | PixelFormatAlpha | PixelFormatPAlpha | PixelFormatGDI),
      PixelFormat48bppRGB=(12 | (48 << 8) | PixelFormatExtended),
      PixelFormat64bppARGB=(13 | (64 << 8) | PixelFormatAlpha | PixelFormatCanonical | PixelFormatExtended),
      PixelFormat64bppPARGB=(14 | (64 << 8) | PixelFormatAlpha | PixelFormatPAlpha | PixelFormatExtended),
      PixelFormatMax=15
   }

   // Pulled from imaging.h in the Windows Mobile 5.0 Pocket PC SDK
   enum BufferDisposalFlag : int
   {
      BufferDisposalFlagNone,
      BufferDisposalFlagGlobalFree,
      BufferDisposalFlagCoTaskMemFree,
      BufferDisposalFlagUnmapView
   }

   // Pulled from imaging.h in the Windows Mobile 5.0 Pocket PC SDK
   enum InterpolationHint : int
   {
      InterpolationHintDefault,
      InterpolationHintNearestNeighbor,
      InterpolationHintBilinear,
      InterpolationHintAveraging,
      InterpolationHintBicubic
   }

   [Flags]
   enum ImageLockMode
   {
      ImageLockModeRead=0x0001,
      ImageLockModeWrite=0x0002,
      ImageLockModeUserInputBuf=0x0004,
   };

#pragma warning disable 0649
   // Pulled from gdiplusimaging.h in the Windows Mobile 5.0 Pocket PC SDK
   struct BitmapImageData
   {
      public uint Width;
      public uint Height;
      public int Stride;
      public PixelFormatID PixelFormat;
      public IntPtr Scan0;
      public IntPtr Reserved;
   }

   // Pulled from imaging.h in the Windows Mobile 5.0 Pocket PC SDK
   struct ImageInfo
   {
      public uint GuidPart1;  // I am being lazy here, I don't care at this point about the RawDataFormat GUID
      public uint GuidPart2;  // I am being lazy here, I don't care at this point about the RawDataFormat GUID
      public uint GuidPart3;  // I am being lazy here, I don't care at this point about the RawDataFormat GUID
      public uint GuidPart4;  // I am being lazy here, I don't care at this point about the RawDataFormat GUID
      public PixelFormatID pixelFormat;
      public int Width;
      public int Height;
      public uint TileWidth;
      public uint TileHeight;
      public double Xdpi;
      public double Ydpi;
      public uint Flags;
   }
#pragma warning restore 0649

   // Pulled from imaging.h in the Windows Mobile 5.0 Pocket PC SDK
   [ComImport, Guid("327ABDA7-072B-11D3-9D7B-0000F81EF32E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
   [ComVisible(true)]
   interface IImagingFactory
   {
      uint CreateImageFromStream();       // This is a place holder, note the lack of arguments
      uint CreateImageFromFile(string filename, out IImage image);
      // We need the MarshalAs attribute here to keep COM interop from sending the buffer down as a Safe Array.
      uint CreateImageFromBuffer([MarshalAs(UnmanagedType.LPArray)] byte[] buffer, uint size, BufferDisposalFlag disposalFlag, out IImage image);
      uint CreateNewBitmap(uint width, uint height, PixelFormatID pixelFormat, out IBitmapImage bitmap);
      uint CreateBitmapFromImage(IImage image, uint width, uint height, PixelFormatID pixelFormat, InterpolationHint hints, out IBitmapImage bitmap);
      uint CreateBitmapFromBuffer();      // This is a place holder, note the lack of arguments
      uint CreateImageDecoder();          // This is a place holder, note the lack of arguments
      uint CreateImageEncoderToStream();  // This is a place holder, note the lack of arguments
      uint CreateImageEncoderToFile();    // This is a place holder, note the lack of arguments
      uint GetInstalledDecoders();        // This is a place holder, note the lack of arguments
      uint GetInstalledEncoders();        // This is a place holder, note the lack of arguments
      uint InstallImageCodec();           // This is a place holder, note the lack of arguments
      uint UninstallImageCodec();         // This is a place holder, note the lack of arguments
   }

   struct RECT
   {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;

      public int Width
      {
         get
         {
            return Right - Left;
         }
         set
         {
            Right = Left + value;
         }
      }

      public int Height
      {
         get
         {
            return Bottom - Top;
         }
         set
         {
            Bottom = Top + value;
         }
      }

      public RECT(System.Drawing.Rectangle rect)
      {
         Left = rect.Left;
         Top = rect.Top;
         Right = rect.Right;
         Bottom = rect.Bottom;
      }

      public RECT(int left, int top, int width, int height)
      {
         Left = left;
         Top = top;
         Right = left + width;
         Bottom = top + height;
      }

      public static implicit operator RECT(System.Drawing.Rectangle rect)
      {
         return new RECT(rect);
      }
   }

   // Pulled from imaging.h in the Windows Mobile 5.0 Pocket PC SDK
   [ComImport, Guid("327ABDA9-072B-11D3-9D7B-0000F81EF32E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
   [ComVisible(true)]
   interface IImage
   {
      uint GetPhysicalDimension(out Size size);
      uint GetImageInfo(out ImageInfo info);
      uint SetImageFlags(uint flags);
      uint Draw(IntPtr hdc, ref RECT dstRect, IntPtr srcRect); // "Correct" declaration: uint Draw(IntPtr hdc, ref Rectangle dstRect, ref Rectangle srcRect);
      uint PushIntoSink();    // This is a place holder, note the lack of arguments
      uint GetThumbnail(uint thumbWidth, uint thumbHeight, out IImage thumbImage);
   }

   // Pulled from imaging.h in the Windows Mobile 5.0 Pocket PC SDK
   [ComImport, Guid("327ABDAA-072B-11D3-9D7B-0000F81EF32E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
   [ComVisible(true)]
   interface IBitmapImage
   {
      uint GetSize(out Size size);
      uint GetPixelFormatID(out PixelFormatID pixelFormat);
      uint LockBits(ref RECT rect, ImageLockMode flags, PixelFormatID pixelFormat, out BitmapImageData lockedBitmapData);
      uint UnlockBits(ref BitmapImageData lockedBitmapData);
      uint GetPalette();  // This is a place holder, note the lack of arguments
      uint SetPalette();  // This is a place holder, note the lack of arguments
   }

   #endregion
}