
namespace GMap.NET.WindowsPresentation
{
   using System.Collections.Generic;
   using System.Collections.ObjectModel;
   using System.Diagnostics;
   using System.Windows;
   using System.Windows.Media;
   using System.Windows.Media.Imaging;
   using GMap.NET.Internals;

   internal class TileVisual : FrameworkElement
   {
      public readonly ObservableCollection<ImageSource> Source;
      public readonly RawTile Tile;

      public TileVisual(IEnumerable<ImageSource> src, RawTile tile)
      {
         Opacity = 0;
         Tile = tile;

         Source = new ObservableCollection<ImageSource>(src);
         Source.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Source_CollectionChanged);

         this.Loaded += new RoutedEventHandler(ImageVisual_Loaded);
         this.Unloaded += new RoutedEventHandler(ImageVisual_Unloaded);
      }

      void Source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         if(IsLoaded)
         {
            switch(e.Action)
            {
               case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
               case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
               case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
               case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
               {
                  UpdateVisual();
               }
               break;

               case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
               {
                  Child = null;
               }
               break;
            }
         }
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

      static readonly Pen gridPen = new Pen(Brushes.White, 2.0);

      private DrawingVisual Create()
      {
         var dv = new DrawingVisual();

         using(DrawingContext dc = dv.RenderOpen())
         {
            foreach(var img in Source)
            {
               var rect = new Rect(0, 0, img.Width + 0.6, img.Height + 0.6);

               dc.DrawImage(img, rect);
               dc.DrawRectangle(null, gridPen, rect);
            }
         }

         return dv;
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
