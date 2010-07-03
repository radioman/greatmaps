
namespace GMap.NET.WindowsForms
{
   using System.Drawing;
   using GMap.NET.WindowsForms.ToolTips;
   using System.Windows.Forms;

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
      GMapOverlay overlay;
      public GMapOverlay Overlay
      {
         get
         {
            return overlay;
         }
         internal set
         {
            overlay = value;
         }
      }

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

            if(Overlay != null && Overlay.Control != null)
            {
               GMap.NET.Point p = Overlay.Control.FromLatLngToLocal(Position);
               LocalPosition = new Point(p.X + Offset.X, p.Y  + Offset.Y);
            }
         }
      }

      internal void ForceUpdateLocalPosition()
      {
         if(Overlay != null && Overlay.Control != null)
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

      /// <summary>
      /// marker position in local coordinates, internal only, do not set it manualy
      /// </summary>
      public Point LocalPosition
      {
         get
         {
            return area.Location;
         }
         set
         {
            if(area.Location != value)
            {
               area.Location = value;

               if(Overlay != null && Overlay.Control != null)
               {
                  if(!Overlay.Control.HoldInvalidation)
                  {
                     Overlay.Control.Core_OnNeedInvalidation();
                  }
               }
            }
         }
      }

      /// <summary>
      /// ToolTip position in local coordinates
      /// </summary>
      public Point ToolTipPosition
      {
         get
         {
            Point ret = area.Location;
            ret.Offset(-Offset.X, -Offset.Y);
            return ret;
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
            return area;
         }
      }

      public GMapToolTip ToolTip;

      public MarkerTooltipMode ToolTipMode = MarkerTooltipMode.OnMouseOver;

      string toolTipText;
      public string ToolTipText
      {
         get
         {
            return toolTipText;
         }

         set
         {
            if(ToolTip == null)
            {
#if !PocketPC
               ToolTip = new GMapRoundedToolTip(this);
#else
               ToolTip = new GMapToolTip(this);
#endif
            }
            toolTipText = value;
         }
      }

      /// <summary>
      /// is marker visible
      /// </summary>
      public bool Visible = true;

      /// <summary>
      /// if true, marker will be rendered even if it's outside current view
      /// </summary>
      public bool DisableRegionCheck = false;

      /// <summary>
      /// can maker receive input
      /// </summary>
      public bool IsHitTestVisible = true;

      private bool isMouseOver = false;

      /// <summary>
      /// is mouse over marker
      /// </summary>
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

   public delegate void MarkerClick(GMapMarker item, MouseEventArgs e);
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
