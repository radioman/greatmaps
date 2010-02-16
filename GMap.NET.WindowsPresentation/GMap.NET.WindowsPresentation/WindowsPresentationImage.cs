
namespace GMap.NET.WindowsPresentation
{
   using System.Windows.Media;
   using System.Windows.Media.Imaging;
   using GMap.NET.Internals;
   using System.Windows;
   using System.Diagnostics;

   internal class TileVisual : FrameworkElement
   {
      public readonly ImageSource[] Source;
      public readonly RawTile Tile;

      public TileVisual(ImageSource[] src, RawTile tile)
      {
         Opacity = 0;
         Source = src;
         Tile = tile;

         this.Loaded += new RoutedEventHandler(ImageVisual_Loaded);
         this.Unloaded += new RoutedEventHandler(ImageVisual_Unloaded);       
      }

      void ImageVisual_Unloaded(object sender, RoutedEventArgs e)
      {
         Child = null;
      }

      void ImageVisual_Loaded(object sender, RoutedEventArgs e)
      {
         UpdateVisual();
      }

      Visual _child;
      public virtual Visual Child
      {
         get
         {
            return _child;
         }
         set
         {
            if(_child != value)
            {
               if(_child != null)
               {
                  RemoveLogicalChild(_child);
                  RemoveVisualChild(_child);
               }

               if(value != null)
               {
                  AddVisualChild(value);
                  AddLogicalChild(value);
               }

               // cache the new child
               _child = value;

               InvalidateVisual();
            }
         }
      }

      public void UpdateVisual()
      {
         Child = Create();
      }

      private DrawingVisual Create()
      {
         var square = new DrawingVisual();

         using(DrawingContext dc = square.RenderOpen())
         {
            foreach(var img in Source)
            {
               dc.DrawImage(img, new Rect(new Size(img.Width + 0.5, img.Height + 0.5)));
            }
         }

         return square;
      }

      #region Necessary Overrides -- Needed by WPF to maintain bookkeeping of our hosted visuals
      protected override int VisualChildrenCount
      {
         get
         {
            return (Child == null ? 0 : 1);
         }
      }

      protected override Visual GetVisualChild(int index)
      {
         Debug.Assert(index == 0);
         return Child;
      }
      #endregion
   }     

   /// <summary>
   /// image abstraction
   /// </summary>
   public class WindowsPresentationImage : PureImage
   {
      public ImageSource Img;

      public override void Dispose()
      {
         if(Img != null)
         {
            Img = null;             
         }

         if(Data != null)
         {
            Data.Dispose();
            Data = null;
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

               m = null;
               bitmapDecoder = null;
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

                  m = null;
                  bitmapDecoder = null;                    
               }
               catch
               {
                  ret = null;
               }
            }
            finally
            {
               try
               {
                  stream.Seek(0, System.IO.SeekOrigin.Begin);

                  if(ret == null)
                  {
                     stream.Dispose();
                  }
               }
               catch
               {
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
