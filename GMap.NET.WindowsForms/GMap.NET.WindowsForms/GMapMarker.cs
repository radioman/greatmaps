
namespace GMap.NET.WindowsForms
{
   using System.Drawing;

   /// <summary>
   /// GMap.NET marker
   /// </summary>
   public class GMapMarker
   {
#if PocketPC
      static System.Drawing.Imaging.ImageAttributes attr = new System.Drawing.Imaging.ImageAttributes();

      static GMapMarker()
      {
         attr.SetColorKey(Color.White, Color.White);
      }
#endif
      internal GMapOverlay Overlay;

      private PointLatLng position;
      public PointLatLng Position
      {
         get
         {
            return position;
         }
         set
         {
            position = value;

            if(Overlay != null)
            {
               GMap.NET.Point p = Overlay.Control.FromLatLngToLocal(Position);
               LocalPosition = new Point(p.X + Offset.X, p.Y  + Offset.Y);
            }
         }
      }

      internal void ForceUpdateLocalPosition()
      {
         if(Overlay != null)
         {
            GMap.NET.Point p = Overlay.Control.FromLatLngToLocal(Position);
            area.Location = new Point(p.X + Offset.X, p.Y  + Offset.Y);
         }
      }

      public object Tag;

      Point offset;
      public Point Offset
      {
         get
         {
            return offset;
         }
         set
         {
            offset = value;
         }
      }

      Rectangle area;
      public Point LocalPosition
      {
         get
         {
            return area.Location;
         }
         internal set
         {
            if(area.Location != value)
            {
               area.Location = value;

               if(Overlay != null)
               {
                  Overlay.Control.Core_OnNeedInvalidation();
               }
            }
         }
      }

      public Size Size
      {
         get
         {
            return area.Size;
         }
         set
         {
            area.Size = value;
         }
      }

      public Rectangle LocalArea
      {
         get
         {
            Rectangle ret = area;
            ret.Offset(-Size.Width/2, -Size.Height/2);
            return ret;
         }
      }

      public GMapToolTip ToolTip = null;
      public MarkerTooltipMode ToolTipMode = MarkerTooltipMode.OnMouseOver;
      public string ToolTipText
      {
          get
          {
              if (ToolTip != null)
                  return ToolTip.ToolTipText;
              return "";
          }

          set
          {
              if (ToolTip != null)
                  ToolTip.ToolTipText = value;
          }
      }

       public bool Visible = true;

      private bool isMouseOver = false;
      public bool IsMouseOver
      {
         get
         {
            return isMouseOver;
         }
         internal set
         {
            isMouseOver = value;

            Overlay.Control.IsMouseOverMarker = value;
         }
      }

      public GMapMarker(PointLatLng pos)
      {
          this.Position = pos;
          this.ToolTip = CreateAndAttachToolTip();
      }

      public virtual GMapToolTip CreateAndAttachToolTip()
      {
          return new GMapToolTip(this);
      }

      public virtual void OnRender(Graphics g)
      {
         //
      }

#if PocketPC
      protected void DrawImageUnscaled(Graphics g, Bitmap inBmp, int x, int y)
      {
         g.DrawImage(inBmp, new Rectangle(x, y, inBmp.Width, inBmp.Height), 0, 0, inBmp.Width, inBmp.Height, GraphicsUnit.Pixel, attr);
      }
#endif
   }

   public delegate void MarkerClick(GMapMarker item);
   public delegate void MarkerEnter(GMapMarker item);
   public delegate void MarkerLeave(GMapMarker item);

   /// <summary>
   /// modeof tooltip
   /// </summary>
   public enum MarkerTooltipMode
   {
      OnMouseOver,
      Never,
      Always,
   }
}
