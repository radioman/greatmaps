
namespace GMap.NET.WindowsForms
{
   using GMap.NET;
   using System.Collections.Generic;
   using System.Drawing;
   using System.Drawing.Drawing2D;

   /// <summary>
   /// GMap.NET polygon
   /// </summary>
   public class GMapPolygon : MapRoute
   {
      /// <summary>
      /// specifies how the outline is painted
      /// </summary>
#if !PocketPC
      public Pen Stroke = new Pen(Color.FromArgb(155, Color.MidnightBlue));
#else
      public Pen Stroke = new Pen(Color.MidnightBlue);
#endif

      /// <summary>
      /// background color
      /// </summary>
#if !PocketPC
      public Brush Fill = new SolidBrush(Color.FromArgb(155, Color.AliceBlue));
#else
      public Brush Fill = new System.Drawing.SolidBrush(Color.AliceBlue);
#endif

      public readonly List<GMap.NET.Point> LocalPoints = new List<GMap.NET.Point>();

      public GMapPolygon(List<PointLatLng> points, string name)
         : base(points, name)
      {
         LocalPoints.Capacity = Points.Count;

#if !PocketPC
         Stroke.LineJoin = LineJoin.Round;
#endif
         Stroke.Width = 5;
      }
   }
}
