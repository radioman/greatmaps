using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace GMapNET
{
   /// <summary>
   /// GMap.NET marker
   /// </summary>
   public class GMapMarker : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;
      public void OnPropertyChanged(string name)
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
               GMapNET.Point p = Overlay.Control.FromLatLngToLocal(value);
               Point pl = new Point(p.X, p.Y);
               pl.Offset(Offset.X, Offset.Y);
               LocalPosition = pl;
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

      private bool isDragging;
      public bool IsDragging
      {
         get
         {
            return isDragging;
         }
         internal set
         {
            isDragging = value;
         }
      }

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
         this.IsDragging = false;
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
   }

   public delegate void MarkerClick(GMapMarker item);
   public delegate void MarkerEnter(GMapMarker item);
   public delegate void MarkerLeave(GMapMarker item);

   public enum MarkerTooltipMode
   {
      OnMouseOver,
      Never,
      Always,
   }
}
