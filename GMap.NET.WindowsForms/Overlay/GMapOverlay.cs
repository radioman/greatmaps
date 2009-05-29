using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GMapNET
{
   /// <summary>
   /// GMap.NET overlay
   /// </summary>
   public class GMapOverlay
   {
      /// <summary>
      /// is overlay visible
      /// </summary>
      public bool IsVisibile = true;

      /// <summary>
      /// overlay Id
      /// </summary>
      public string Id;

      /// <summary>
      /// list of markers, should be thread safe
      /// </summary>
      public readonly ObservableCollectionThreadSafe<GMapMarker> Markers = new ObservableCollectionThreadSafe<GMapMarker>();

      /// <summary>
      /// list of routes, should be thread safe
      /// </summary>
      public readonly ObservableCollectionThreadSafe<MapRoute> Routes = new ObservableCollectionThreadSafe<MapRoute>();

      /// <summary>
      /// font for markers tooltip
      /// </summary>
      public Font TooltipFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold, GraphicsUnit.Point);

      /// <summary>
      /// tooltip pen
      /// </summary>
      public Pen TooltipPen = new Pen(Color.FromArgb(140, Color.MidnightBlue));

      /// <summary>
      /// tooltip background color
      /// </summary>
      public Brush TooltipBackground = Brushes.AliceBlue;

      /// <summary>
      /// tooltip string format
      /// </summary>
      public StringFormat TooltipFormat = new StringFormat();

      Pen RoutePen = new Pen(Color.MidnightBlue);
      internal GMap Control;            

      public GMapOverlay(GMap control, string id)
      {
         Control = control;
         Id = id;
         Markers.CollectionChanged += new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
         Routes.CollectionChanged += new NotifyCollectionChangedEventHandler(Routes_CollectionChanged);

         RoutePen.LineJoin = LineJoin.Round;
         RoutePen.Width = 5;

         TooltipPen.Width = 2;
         TooltipPen.LineJoin = LineJoin.Round;
         TooltipPen.StartCap = LineCap.RoundAnchor;

         TooltipFormat.Alignment     = StringAlignment.Center;
         TooltipFormat.LineAlignment = StringAlignment.Center;
      }

      void Routes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if(e.NewItems != null)
         {
            foreach(MapRoute obj in e.NewItems)
            {
               Control.UpdateRouteLocalPosition(obj);
            }
         }  

         Control.Core_OnNeedInvalidation();
      }

      void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if(e.NewItems != null)
         {
            foreach(GMapMarker obj in e.NewItems)
            {
               if(obj != null)
               {
                  obj.Overlay = this;
                  obj.Position = obj.Position;
               }
            }
         }

         Control.Core_OnNeedInvalidation();
      }

      /// <summary>
      /// draws tooltip, override to draw custom
      /// </summary>
      /// <param name="g"></param>
      /// <param name="pos"></param>
      protected virtual void DrawToolTip(Graphics g, GMapMarker m, int x, int y)
      {
         GraphicsState s = g.Save();
         g.SmoothingMode = SmoothingMode.AntiAlias;

         System.Drawing.Size st = g.MeasureString(m.ToolTipText, TooltipFont).ToSize();
         System.Drawing.Rectangle rect = new System.Drawing.Rectangle(x, y, st.Width+Control.TooltipTextPadding.Width, st.Height+Control.TooltipTextPadding.Height);
         rect.Offset(m.ToolTipOffset.X, m.ToolTipOffset.Y);

         g.DrawLine(TooltipPen, x, y, rect.X + rect.Width/2, rect.Y + rect.Height/2);
         g.FillRectangle(TooltipBackground, rect);
         g.DrawRectangle(TooltipPen, rect);
         g.DrawString(m.ToolTipText, TooltipFont, Brushes.Navy, rect, TooltipFormat);

         g.Restore(s);
      }

      /// <summary>
      /// draw routes, override to draw custom
      /// </summary>
      /// <param name="g"></param>
      protected virtual void DrawRoutes(Graphics g)
      {
         GraphicsState st = g.Save();
         g.SmoothingMode = SmoothingMode.AntiAlias;

         foreach(GMapRoute r in Routes)
         {
            RoutePen.Color = r.Color;

            using(GraphicsPath rp = new GraphicsPath())
            {
               for(int i = 0; i < r.LocalPoints.Count; i++)
               {
                  GMapNET.Point p2 = r.LocalPoints[i];

                  if(i == 0)
                  {
                     rp.AddLine(p2.X, p2.Y, p2.X, p2.Y);
                  }
                  else
                  {
                     System.Drawing.PointF p = rp.GetLastPoint();
                     rp.AddLine(p.X, p.Y, p2.X, p2.Y);
                  }
               }

               if(rp.PointCount > 0)
               {
                  g.DrawPath(RoutePen, rp);
               }
            }
         }

         g.Restore(st);
      }

      /// <summary>
      /// renders objects and routes
      /// </summary>
      /// <param name="g"></param>
      internal void Render(Graphics g)
      {
         if(Control.RoutesEnabled && !(Form.ModifierKeys == Keys.Control))
         {
            DrawRoutes(g);
         }

         if(Control.MarkersEnabled && !(Form.ModifierKeys == Keys.Control))
         {
            // markers
            foreach(GMapMarker m in Markers)
            {
               if(m.Visible && Control.Core.CurrentRegion.Contains(m.LocalPosition.X, m.LocalPosition.Y))
               {
                  m.OnRender(g);
               }
            }

            // tooltips above
            foreach(GMapMarker m in Markers)
            {
               if(m.Visible && Control.Core.CurrentRegion.Contains(m.LocalPosition.X, m.LocalPosition.Y))
               {
                  if(!string.IsNullOrEmpty(m.ToolTipText))
                  {
                     if(m.TooltipMode == MarkerTooltipMode.Always || (m.TooltipMode == MarkerTooltipMode.OnMouseOver && m.IsMouseOver))
                     {
                        DrawToolTip(g, m, m.LocalPosition.X, m.LocalPosition.Y);
                     }
                  }
               }
            }
         }
      }
   }
}