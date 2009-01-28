using System;
using System.Drawing;

namespace GMapNET.Internals
{
   public class Tile
   {
      IntPtr ptrHbitmap;
      PureImage image;
      RenderMode mode;
      Point pos;
      int zoom;

      public Tile(PureImage image, int zoom, Point pos, RenderMode mode)
      {
         if(image != null)
         {
            this.mode = mode;
            this.Zoom = zoom;
            this.Pos = pos;

            switch(mode)
            {
               case RenderMode.GDI:
               {
                  this.Hbitmap = image.GetHbitmap();
               }
               break;

               case RenderMode.GDI_PLUS:
               case RenderMode.WPF:
               {
                  this.Image = image;
               }
               break;
            }
         }
      }

      public void Clear()
      {
         switch(mode)
         {
            case RenderMode.GDI:
            {
               if(Hbitmap != IntPtr.Zero)
               {
                  NativeMethods.DeleteObject(Hbitmap);
                  Hbitmap = IntPtr.Zero;
               }
            }
            break;

            case RenderMode.GDI_PLUS:
            case RenderMode.WPF:
            {
               if(Image != null)
               {
                  Image.Dispose();
                  Image = null;
               }
            }
            break;
         }
      }

      public PureImage Image
      {
         get
         {
            return image;
         }
         private set
         {
            image = value;
         }
      }

      public IntPtr Hbitmap
      {
         get
         {
            return ptrHbitmap;
         }
         private set
         {
            ptrHbitmap = value;
         }
      }

      public int Zoom
      {
         get
         {
            return zoom;
         }
         private set
         {
            zoom = value;
         }
      }

      public Point Pos
      {
         get
         {
            return pos;
         }
         private set
         {
            pos= value;
         }
      }
   }
}
