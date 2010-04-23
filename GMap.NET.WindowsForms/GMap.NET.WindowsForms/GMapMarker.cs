
namespace GMap.NET.WindowsForms
{
   using System.ComponentModel;
   using System.Drawing;

   /// <summary>
   /// GMap.NET marker
   /// </summary>
   public class GMapMarker : INotifyPropertyChanged
   {
#if PocketPC
      System.Drawing.Imaging.ImageAttributes attr = new System.Drawing.Imaging.ImageAttributes();

      public GMapMarker()
      {
         attr.SetColorKey(Color.White, Color.White);
      }
#endif

      public event PropertyChangedEventHandler PropertyChanged;
      void OnPropertyChanged(string name)
      {
         PropertyChangedEventHandler handler = PropertyChanged;
         if(handler != null)
         {
            handler(this, new PropertyChangedEventArgs(name));
         }
      }

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
               GMap.NET.Point p = Overlay.Control.FromLatLngToLocal(value);
               LocalPosition = new Point(p.X + Offset.X, p.Y  + Offset.Y);
            }
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
            area.Location = value;
            OnPropertyChanged("LocalPosition");

            if(Overlay != null)
            {
               Overlay.Control.Core_OnNeedInvalidation();
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

      internal Rectangle LocalArea
      {
         get
         {
            Rectangle ret = area;
            ret.Offset(-Size.Width/2, -Size.Height/2);
            return ret;
         }
      }

      public MarkerTooltipMode TooltipMode;
      public Point ToolTipOffset;
      public string ToolTipText;
      public bool Visible;

      private bool isMouseOver;
      public bool IsMouseOver
      {
         get
         {
            return isMouseOver;
         }
         internal set
         {
            isMouseOver = value;
         }
      }

      public GMapMarker(PointLatLng pos)
      {
         this.Position = pos;
         this.ToolTipText = string.Empty;
         this.TooltipMode = MarkerTooltipMode.OnMouseOver;
         this.Visible = true;

         this.IsMouseOver = false;
         this.ToolTipOffset = new Point(14, -44);
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
