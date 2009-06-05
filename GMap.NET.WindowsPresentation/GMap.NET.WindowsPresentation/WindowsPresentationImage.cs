
namespace GMap.NET.WindowsPresentation
{
   using System.Windows.Media;
   using System.Windows.Media.Imaging;

   /// <summary>
   /// image abstraction
   /// </summary>
   public class WindowsPresentationImage : PureImage
   {
      public ImageSource Img;

      public override object Clone()
      {
         if(Img != null)
         {
            WindowsPresentationImage ret = new WindowsPresentationImage();
            ret.Img = Img.CloneCurrentValue();
            if(ret.Img.CanFreeze)
            {
               ret.Img.Freeze();
            }
            return ret;
         }
         return null;
      }

      public override void Dispose()
      {
         if(Img != null)
         {
            Img = null;
         }
      }
   }

   /// <summary>
   /// image abstraction proxy
   /// </summary>
   public class WindowsPresentationImageProxy : PureImageProxy
   {
      public override PureImage FromStream(System.IO.Stream stream)
      {
         WindowsPresentationImage ret = null;
         if(stream != null)
         {
            // try png decoder
            try
            {
               PngBitmapDecoder bitmapDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
               ImageSource m = bitmapDecoder.Frames[0];

               if(m != null)
               {
                  ret = new WindowsPresentationImage();
                  ret.Img = m;
                  if(ret.Img.CanFreeze)
                  {
                     ret.Img.Freeze();
                  }
               }
            }
            catch
            {
               // try jpeg decoder
               try
               {
                  stream.Seek(0, System.IO.SeekOrigin.Begin);

                  JpegBitmapDecoder bitmapDecoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                  ImageSource m = bitmapDecoder.Frames[0];

                  if(m != null)
                  {
                     ret = new WindowsPresentationImage();
                     ret.Img = m;
                     if(ret.Img.CanFreeze)
                     {
                        ret.Img.Freeze();
                     }
                  }
               }
               catch
               {
                  ret = null;
               }
            }
         }
         return ret;
      }

      public override bool Save(System.IO.Stream stream, PureImage image)
      {
         WindowsPresentationImage ret = (WindowsPresentationImage) image;
         if(ret.Img != null)
         {
            try
            {
               PngBitmapEncoder e = new PngBitmapEncoder();
               e.Frames.Add(BitmapFrame.Create(ret.Img as BitmapSource));
               e.Save(stream);
            }
            catch
            {
               return false;
            }
         }
         else
         {
            return false;
         }

         return true;
      }
   }
}
