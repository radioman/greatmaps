
namespace GMap.NET.WindowsForms
{
   using System.Drawing;
   using System.Drawing.Drawing2D;
   using System.Windows.Forms;
   using GMap.NET.ObjectModel;

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
      public readonly ObservableCollectionThreadSafe<GMapRoute> Routes = new ObservableCollectionThreadSafe<GMapRoute>();

      /// <summary>
      /// list of polygons, should be thread safe
      /// </summary>
      public readonly ObservableCollectionThreadSafe<GMapPolygon> Polygons = new ObservableCollectionThreadSafe<GMapPolygon>();

      /// <summary>
      /// font for markers tooltip
      /// </summary>
#if !PocketPC
      public Font TooltipFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold, GraphicsUnit.Pixel);
#else
      public Font TooltipFont = new Font(FontFamily.GenericSansSerif, 6, FontStyle.Bold);
#endif

      /// <summary>
      /// tooltip pen
      /// </summary>
#if !PocketPC
      public Pen TooltipPen = new Pen(Color.FromArgb(140, Color.MidnightBlue));
#else
      public Pen TooltipPen = new Pen(Color.MidnightBlue);
#endif

      /// <summary>
      /// tooltip background color
      /// </summary>
#if !PocketPC
      public Brush TooltipBackground = Brushes.AliceBlue;
#else
      public Brush TooltipBackground = new System.Drawing.SolidBrush(Color.AliceBlue);
      public Brush TooltipForeground = new System.Drawing.SolidBrush(Color.Navy);
#endif

      /// <summary>
      /// Polygon background color
      /// </summary>
#if !PocketPC
      public Brush PolygonBackground = new SolidBrush(Color.FromArgb(155, Color.AliceBlue));
#else
      public Brush PolygonBackground = new System.Drawing.SolidBrush(Color.AliceBlue);
#endif

      /// <summary>
      /// tooltip string format
      /// </summary>
      public StringFormat TooltipFormat = new StringFormat();

      /// <summary>
      /// pen for routes, be aware that the color is adjusted in each GMapRoute
      /// </summary>
      public Pen RoutePen = new Pen(Color.MidnightBlue);

      internal System.Windows.Forms.GMapControl Control;

      public GMapOverlay(System.Windows.Forms.GMapControl control, string id)
      {
         Control = control;
         Id = id;
         Markers.CollectionChanged += new NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
         Routes.CollectionChanged += new NotifyCollectionChangedEventHandler(Routes_CollectionChanged);
         Polygons.CollectionChanged += new NotifyCollectionChangedEventHandler(Polygons_CollectionChanged);

#if !PocketPC
         RoutePen.LineJoin = LineJoin.Round;
#endif
         RoutePen.Width = 5;

         TooltipPen.Width = 2;
#if !PocketPC
         TooltipPen.LineJoin = LineJoin.Round;
         TooltipPen.StartCap = LineCap.RoundAnchor;
#endif

         TooltipFormat.Alignment     = StringAlignment.Center;
         TooltipFormat.LineAlignment = StringAlignment.Center;
      }

      void Polygons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if(e.NewItems != null)
         {
            foreach(GMapPolygon obj in e.NewItems)
            {
               Control.UpdatePolygonLocalPosition(obj);
            }
         }

         Control.Core_OnNeedInvalidation();
      }

      void Routes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if(e.NewItems != null)
         {
            foreach(GMapRoute obj in e.NewItems)
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
      /// <param name="m"></param>
      /// <param name="x"></param>
      /// <param name="y"></param>
      protected virtual void DrawToolTip(Graphics g, GMapMarker m, int x, int y)
      {
#if !PocketPC
         GraphicsState s = g.Save();
         g.SmoothingMode = SmoothingMode.AntiAlias;
#endif

         System.Drawing.Size st = g.MeasureString(m.ToolTipText, TooltipFont).ToSize();
         System.Drawing.Rectangle rect = new System.Drawing.Rectangle(x, y, st.Width+Control.TooltipTextPadding.Width, st.Height+Control.TooltipTextPadding.Height);
         rect.Offset(m.ToolTipOffset.X, m.ToolTipOffset.Y);

         g.DrawLine(TooltipPen, x, y, rect.X + rect.Width/2, rect.Y + rect.Height/2);
         g.FillRectangle(TooltipBackground, rect);
         g.DrawRectangle(TooltipPen, rect);

#if !PocketPC
         g.DrawString(m.ToolTipText, TooltipFont, Brushes.Navy, rect, TooltipFormat);
#else
         g.DrawString(m.ToolTipText, TooltipFont, TooltipForeground, rect, TooltipFormat);
#endif

#if !PocketPC
         g.Restore(s);
#endif
      }

      /// <summary>
      /// draw routes, override to draw custom
      /// </summary>
      /// <param name="g"></param>
      protected virtual void DrawRoutes(Graphics g)
      {
#if !PocketPC
         GraphicsState st = g.Save();
         g.SmoothingMode = SmoothingMode.AntiAlias;

         foreach(GMapRoute r in Routes)
         {
            RoutePen.Color = r.Color;

            using(GraphicsPath rp = new GraphicsPath())
            {
               for(int i = 0; i < r.LocalPoints.Count; i++)
               {
                  GMap.NET.Point p2 = r.LocalPoints[i];

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
#else
         foreach(GMapRoute r in Routes)
         {
            RoutePen.Color = r.Color;

            Point[] pnts = new Point[r.LocalPoints.Count];
            for(int i = 0; i < r.LocalPoints.Count; i++)
            {
               Point p2 = new Point(r.LocalPoints[i].X, r.LocalPoints[i].Y);
               pnts[pnts.Length - 1 - i] = p2;
            }

            if(pnts.Length > 0)
            {
               g.DrawLines(RoutePen, pnts);
            }
         }
#endif
      }

      /// <summary>
      /// draw polygons, override to draw custom
      /// </summary>
      /// <param name="g"></param>
      protected virtual void DrawPolygons(Graphics g)
      {
#if !PocketPC
         GraphicsState st = g.Save();
         g.SmoothingMode = SmoothingMode.AntiAlias;

         foreach(GMapPolygon r in Polygons)
         {
            RoutePen.Color = r.Color;

            using(GraphicsPath rp = new GraphicsPath())
            {
               for(int i = 0; i < r.LocalPoints.Count; i++)
               {
                  GMap.NET.Point p2 = r.LocalPoints[i];

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
                  rp.CloseFigure();

                  g.FillPath(PolygonBackground, rp);

                  g.DrawPath(RoutePen, rp);
               }
            }
         }
         g.Restore(st);
#else
         foreach(GMapPolygon r in Polygons)
         {
            RoutePen.Color = r.Color;

            Point[] pnts = new Point[r.LocalPoints.Count];
            for(int i = 0; i < r.LocalPoints.Count; i++)
            {
               Point p2 = new Point(r.LocalPoints[i].X, r.LocalPoints[i].Y);
               pnts[pnts.Length - 1 - i] = p2;
            }

            if(pnts.Length > 0)
            {
               g.FillPolygon(PolygonBackground, pnts);
               g.DrawPolygon(RoutePen, pnts);
            }
         }
#endif
      }

      /// <summary>
      /// renders objects and routes
      /// </summary>
      /// <param name="g"></param>
      public virtual void Render(Graphics g)
      {
         if(Control.PolygonsEnabled)
         {
            DrawPolygons(g);
         }

         if(Control.RoutesEnabled)
         {
            DrawRoutes(g);
         }

         if(Control.MarkersEnabled)
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